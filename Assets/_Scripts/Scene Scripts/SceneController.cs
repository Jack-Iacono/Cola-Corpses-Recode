using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using System.Linq;

public class SceneController : MonoBehaviour
{
    public static SceneController instance { get; private set; }

    public GameObject loadingScreen;
    private static List<m_Scene> sceneTarget = new List<m_Scene>();
    public static bool loading { get; private set; } = false;

    public enum m_Scene { MAIN_MENU, GAME, GAME_TEST, UNIVERSAL, GAME_SYS };
    public readonly static Dictionary<m_Scene, SceneData> scenes = new Dictionary<m_Scene, SceneData>
        {
            { m_Scene.MAIN_MENU, new SceneData("scn_MainMenu", SceneData.Type.MAP) },
            { m_Scene.GAME, new SceneData("scn_Game", SceneData.Type.MAP) },
            { m_Scene.GAME_TEST, new SceneData("scn_GameTest", SceneData.Type.MAP) },
            { m_Scene.UNIVERSAL, new SceneData("scn_UniversalFunction", SceneData.Type.UTIL) },
            { m_Scene.GAME_SYS, new SceneData("scn_GameSystems", SceneData.Type.UTIL) }
        };
    private static List<string> mapScenes = new List<string>();

    public delegate void OnMapLoadedDelegate(string mapName);
    public static event OnMapLoadedDelegate OnMapLoaded;
    public static Scene loadedMap;

    public delegate void OnTargetChangeDelegate(List<m_Scene> list, bool fromNetwork = false);
    public static event OnTargetChangeDelegate OnSceneTargetChange;

    private void Awake()
    {
        // Just setting the singleton and initializing variables
        if(instance == null)
        {
            instance = this;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            foreach(SceneData s in scenes.Values)
            {
                if(s.type == SceneData.Type.MAP)
                {
                    mapScenes.Add(s.name);
                }
            }
        }
        else
            Destroy(this);
    }

    private void Start()
    {
        // Load a scene when the game is started
        LoadScene(m_Scene.GAME);
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode loadMode)
    {
        // If the scene loaded was a map scene, set it as active
        if(mapScenes.Contains(s.name))
            SceneManager.SetActiveScene(s);

        // Check on the loading status of the scenes
        CheckLoading();
        CheckMapLoaded();
    }
    private void OnSceneUnloaded(Scene s)
    {
        CheckLoading();
    }

    private static void CheckLoading()
    {
        // Make sure that there is a target scene
        if(sceneTarget != null)
        {
            // Get all the loaded scenes' names
            List<string> loadedNames = GetLoadedScenes();
            bool check = true;

            // Run through the scenes and decide which ones to load vs unload
            foreach (m_Scene s in sceneTarget)
            {
                // Load the scene if not present in loaded names, but is present in the list provided
                if (!loadedNames.Contains(scenes[s].name))
                {
                    check = false;
                    loading = true;
                    instance.loadingScreen.SetActive(true);
                    break;
                }
            }

            // See if all scenes match the desired loading scenes
            if (check)
            {
                loading = false;
                instance.loadingScreen.SetActive(false);
            }
        }
    }
    public static void SetLoadScreen(bool b)
    {
        // Show the loading screen based on b
        instance.loadingScreen.SetActive(b);
    }
    public static void SetSceneTarget(List<m_Scene> list, bool fromNetwork = false)
    {
        // Set the list of scenes that should be loaded
        sceneTarget = list;
        OnSceneTargetChange?.Invoke(sceneTarget, fromNetwork);
        CheckLoading();
    }

    protected static void UnloadScene(m_Scene scene, bool offlineOverride = false)
    {
        UnloadScene(scenes[scene].name, offlineOverride);
    }
    protected static void UnloadScene(string scene, bool offlineOverride = false)
    {
        if (SceneManager.GetSceneByName(scene).isLoaded)
        {
            SceneManager.UnloadSceneAsync(scene);
            CheckLoading();
        }
    }

    protected static void LoadScene(m_Scene scene, bool offlineOverride = false)
    {
        LoadScene(scenes[scene].name, offlineOverride);
    }
    protected static void LoadScene(string scene, bool offlineOverride = false)
    {
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        CheckLoading();
    }

    protected static List<string> GetLoadedScenes()
    {
        List<string> loadedNames = new List<string>();
        // Get the list of loaded scenes for easier use
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            loadedNames.Add(SceneManager.GetSceneAt(i).name);
        }
        return loadedNames;
    }

    public static void SetScenes(List<m_Scene> list)
    {
        SetSceneTarget(list);

        List<string> unloadScenes = new List<string>();
        List<string> loadScenes = new List<string>();

        List<string> loadedNames = GetLoadedScenes();

        // run through the scenes and decide which ones to load vs unload
        foreach(m_Scene s in scenes.Keys)
        {
            // Load the scene if not present in loaded names, but is present in the list provided
            if(sceneTarget.Contains(s) && !loadedNames.Contains(scenes[s].name))
            {
                // Load the map first
                if (scenes[s].type == SceneData.Type.MAP)
                    loadScenes.Insert(0, scenes[s].name);
                else
                    loadScenes.Add(scenes[s].name);
            }
            // Unload the scene if present in the loaded names, but not present in the list provided
            else if (!sceneTarget.Contains(s) && loadedNames.Contains(scenes[s].name))
            {
                unloadScenes.Add(scenes[s].name);
            }
        }

        foreach(string uScene in unloadScenes)
        {
            UnloadScene(uScene);
        }
        foreach(string lScene in loadScenes)
        {
            LoadScene(lScene);
        }
    }
    public static void LoadMainMenuScene()
    {
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.MAIN_MENU,
        });
    }
    public static void LoadGameScene()
    {
        // Will load the test game scene if the option is ticked
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.GAME,
            SceneController.m_Scene.GAME_SYS,
            SceneController.m_Scene.UNIVERSAL
        });
    }
    public static void LoadGameTestScene()
    {
        // Will load the test game scene if the option is ticked
        SceneController.SetScenes(new List<SceneController.m_Scene>()
        {
            SceneController.m_Scene.GAME,
            SceneController.m_Scene.GAME_SYS,
            SceneController.m_Scene.UNIVERSAL
        });
    }

    /// <summary>
    /// Checks to see if there is a map scene currently loaded
    /// </summary>
    private void CheckMapLoaded()
    {
        // Run through the scenes that are known by the SceneManager
        for(int i = 0; i < SceneManager.sceneCount; i++)
        {
            // Check if there is any map loaded, and if there is, send event
            if (mapScenes.Contains(SceneManager.GetSceneAt(i).name) && SceneManager.GetSceneAt(i).isLoaded)
            {
                loadedMap = SceneManager.GetSceneAt(i);
                OnMapLoaded?.Invoke(loadedMap.name);
                break;
            }
        }
    }
    public static bool GetSceneLoaded(m_Scene scene)
    {
        for(int i = 0; i < SceneManager.sceneCount;i++)
        {
            if (SceneManager.GetSceneAt(i) == SceneManager.GetSceneByName(scenes[scene].name))
                return true;
        }
        return false;
    }

    public static void SetMapActive()
    {
        if(loadedMap.isLoaded)
            SceneManager.SetActiveScene(loadedMap);
    }
    public static void SetSceneActive(m_Scene scene)
    {
        if(SceneManager.GetSceneByName(scenes[scene].name).isLoaded)
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(scenes[scene].name));
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;   
    }

    public class SceneData
    {
        public string name;
        
        public enum Type { MAP, UTIL }
        public Type type;

        public SceneData(string name, Type type)
        {
            this.name = name;
            this.type = type;
        }
    }
}
