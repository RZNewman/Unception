using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    struct TutorialState
    {
        public List<TutorialWindow> stages;
    }

    struct TutorialWindow
    {
        public FireConditions start;
        public List<TutorialSection> sections;
        public Action<PlayerInfo> action;
        public FinishCondtions stop;
    }

    public struct TutorialSection
    {
        public string displayText;
        public List<Keybinds.KeyName> keybinds;
    }

    struct FireConditions
    {
        public MenuHandler.Menu? menuScreen;
    }
    struct FinishCondtions
    {
        public List<Keybinds.KeyName> keyPress;
        public List<TutorialEvent> events;
        public MenuHandler.Menu? menuScreen;
        public float timer;
        public bool timerIsFallback;
    }

    TutorialState tutorial = new TutorialState
    {
        stages = new List<TutorialWindow>()
        {
            //move
            new TutorialWindow
            {
                start = new FireConditions
                {
                    menuScreen= MenuHandler.Menu.Gameplay
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Move",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.Forward,
                        Keybinds.KeyName.Left,
                        Keybinds.KeyName.Backward,
                        Keybinds.KeyName.Right,
                        }
                    },
                    new TutorialSection
                    {
                        displayText = "Move the Camera",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.CameraRotate,
                        }
                    },
                    new TutorialSection
                    {
                        displayText = "Jump",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.Jump,
                        }
                    },
                    new TutorialSection
                    {
                        displayText = "Dash",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.Dash,
                        }
                    },
                },
                stop = new FinishCondtions
                {
                    keyPress = new List<Keybinds.KeyName>()
                    {
                        Keybinds.KeyName.Forward,
                        Keybinds.KeyName.Left,
                        Keybinds.KeyName.Backward,
                        Keybinds.KeyName.Right,
                        Keybinds.KeyName.Jump,
                        Keybinds.KeyName.Dash,
                    },
                    timer = 60f,
                    timerIsFallback = true,
                }
            },
            //collect water
            new TutorialWindow
            {
                start = new FireConditions
                {
                    
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Collect the water"
                    }
                },
                action = (i) =>
                {
                    Ship  s =FindObjectOfType<Ship>();

                    s.shipWaterArrow.SetActive(true);
                    i.CmdSpawnShipWater(s.shipWaterPosition.transform.position);
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.WaterPickup,
                    }
                }
            },
            //drink water
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Bring the water to the flower"
                    }
                },
                action = (_) =>
                {
                    FindObjectOfType<Ship>().shipWaterArrow.transform.Rotate(Vector3.up,180);
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.WaterFed,
                    }
                }
            },
            //maps
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Go to the wheel"
                    }
                },
                action = (_) =>
                {
                    FindObjectOfType<Ship>().shipWaterArrow.SetActive(false);
                },
                stop = new FinishCondtions
                {
                    menuScreen = MenuHandler.Menu.Map
                }
            },
            //embark
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Select a Map Marker, then Embark!"
                    }
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.MapSelect,
                    }
                }
            },
            //loading..
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    //new TutorialSection
                    //{
                    //    displayText = "Select a Map Marker, then Embark!"
                    //}
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.LoadingFinished,
                    }
                }
            },
            //find water
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Leave to find more water"
                    }
                },
                action = (i) =>
                {
                    Ship  s =FindObjectOfType<Ship>();

                    s.shipTakeoffArrow.SetActive(true);
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.Launch,
                    }
                }
            },
            //goal
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "More water must be here...",
                    }
                },
                action = (i) =>
                {
                    Ship  s =FindObjectOfType<Ship>();

                    s.shipTakeoffArrow.SetActive(false);
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.WaterPickup
                    }
                }
            },
            //recall
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Return to the ship",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.Recall,
                        }
                    }
                },
                stop = new FinishCondtions
                {
                    keyPress = new List<Keybinds.KeyName>()
                    {
                        Keybinds.KeyName.Recall
                    }
                }
            },
            //wait for fed ..
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                },
                stop = new FinishCondtions
                {
                    events = new List<TutorialEvent>()
                    {
                        TutorialEvent.WaterFed
                    }
                }
            },
            //get your items
            new TutorialWindow
            {
                start = new FireConditions
                {
                    
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "You found seeds! Go plant them in the middle",
                    },
                },
                action = (i) => {
                    i.CmdGenMinItems();
                },
                stop = new FinishCondtions
                {
                    menuScreen = MenuHandler.Menu.Loadout,
                }
            },
            //equip
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Click to select your items, and then place them on the board to equip",
                    },
                    new TutorialSection
                    {
                        displayText = "Item Auroras may overlap, but the solid Hardpoints may not",
                    },
                },
                stop = new FinishCondtions
                {
                    menuScreen = MenuHandler.Menu.Gameplay,
                }
            },
            //attack
            new TutorialWindow
            {
                start = new FireConditions
                {
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Attack",
                        keybinds = new List<Keybinds.KeyName>()
                        {
                        Keybinds.KeyName.Attack1,
                        Keybinds.KeyName.Attack2,

                        }
                    }
                },
                stop = new FinishCondtions
                {
                    keyPress = new List<Keybinds.KeyName>()
                    {
                        Keybinds.KeyName.Attack1,
                        Keybinds.KeyName.Attack2,
                    },
                    timer = 45f,
                    timerIsFallback = true,
                }
            },
            
            
        }
    };

    int currentProgress = 0;
    bool isOpen = false;
    TutorialWindow openWindow;

    Keybinds keys;
    UiPopups popups; 
    private void Start()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        keys = FindObjectOfType<Keybinds>();
        FindObjectOfType<MenuHandler>().MenuEvent += OnOpenMenu;
        popups = FindObjectOfType<UiPopups>();
    }


    [Command]
    void CmdSpawnShipWater(Vector3 pos)
    {
        if(GetComponent<PlayerGhost>().power < Atlas.playerStartingPower * 4)
        {
            GameObject w = Instantiate(FindObjectOfType<GlobalPrefab>().WetstonePre, pos, Quaternion.identity);
            w.GetComponent<Reward>().setReward(Atlas.playerStartingPower, 1, 10);
            w.GetComponent<Power>().setPower(Atlas.playerStartingPower);
            w.GetComponent<Power>().setOverrideDefault();
            NetworkServer.Spawn(w);
        }
    }

    [Command]
    void CmdGenMinItems()
    {
        float pow = GetComponent<PlayerGhost>().power;
        if (pow < Atlas.playerStartingPower * 4)
        {
            GetComponent<Inventory>().genMinItems();
        }
    }

    public bool canInteractGrove
    {
        get
        {
            return currentProgress >= 9;
        }
    }


    private void OnDestroy()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        MenuHandler handler = FindObjectOfType<MenuHandler>();
        if (handler)
        {
            handler.MenuEvent -= OnOpenMenu;
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (isOpen)
        {
            lookForKeys();
            if (openWindow.stop.timer>0 && ! openWindow.stop.timerIsFallback)
            {
                openWindow.stop.timer -= Time.deltaTime;
                tryCloseStage();
            }
        }
        else
        {
            if (currentProgress >= tutorial.stages.Count)
            {
                return;
            }
            TutorialWindow nextWindow = tutorial.stages[currentProgress];
            if (!nextWindow.start.menuScreen.HasValue)
            {
                openStage();
            }
        }
    }

    void OnOpenMenu(MenuHandler.Menu menu)
    {
        if (isOpen)
        {
            if(openWindow.stop.menuScreen == menu)
            {
                openWindow.stop.menuScreen = null;
                tryCloseStage();
            }
        }
        else
        {
            if (currentProgress >= tutorial.stages.Count)
            {
                return;
            }
            TutorialWindow nextWindow = tutorial.stages[currentProgress];
            if (nextWindow.start.menuScreen == menu) {
                openStage();
            }
        }
    }

    

    void lookForKeys()
    {
        bool removed = false;
        if (isOpen && openWindow.stop.keyPress != null)
        {
            foreach(Keybinds.KeyName key in openWindow.stop.keyPress.ToList())
            {
                if (Input.GetKey(keys.binding(key)))
                {
                    openWindow.stop.keyPress.Remove(key);
                    removed = true;
                }
            }
            if (removed)
            {
                tryCloseStage();
            }
        }
    }

    public enum TutorialEvent
    {
        WaterPickup,
        WaterFed,
        MapSelect,
        LoadingFinished,
        Launch
    }

    [Server]
    public void FireTutorialEvent(TutorialEvent e)
    {
        TargetFireTutorialEvent(connectionToClient,e);
    }
    [TargetRpc]
    void TargetFireTutorialEvent(NetworkConnection conn ,TutorialEvent e)
    {
        bool removed = false;
        if (isOpen && openWindow.stop.events != null)
        {
            foreach (TutorialEvent tut in openWindow.stop.events.ToList())
            {
                if (tut == e)
                {
                    openWindow.stop.events.Remove(e);
                    removed = true;
                }
            }
            if (removed)
            {
                tryCloseStage();
            }
        }
    }

    [Server]
    public static void FireTutorialEventAll(TutorialEvent e)
    {
        foreach(PlayerInfo pi in FindObjectsOfType<PlayerInfo>())
        {
            pi.FireTutorialEvent(e);
        }
        
    }


    void tryCloseStage()
    {
        bool menuDone = !openWindow.stop.menuScreen.HasValue;
        bool buttonsDone = openWindow.stop.keyPress == null || openWindow.stop.keyPress.Count == 0;
        bool eventsDone = openWindow.stop.events == null || openWindow.stop.events.Count == 0;
        bool timerDone = openWindow.stop.timer <= 0;
        bool timerFallback = openWindow.stop.timerIsFallback;

        if (
            (menuDone && buttonsDone && timerDone && eventsDone)
            ||
            (timerFallback && menuDone && buttonsDone && eventsDone)
            ||
            (timerFallback && timerDone)
            )
        {
            closeStage();
        }
    }

    void closeStage()
    {
        isOpen = false;
        currentProgress++;
        //Debug.Log(currentProgress);
        popups.closePopup();
    }

    void openStage()
    {
        isOpen = true;
        openWindow = tutorial.stages[currentProgress];
        popups.createTutorial(openWindow.sections);
        if(openWindow.action != null)
        {
            openWindow.action(this);
        }
    }

    public struct NotificationsData
    {
        public int tutorialStage;
    }

    public NotificationsData save()
    {
        return new NotificationsData
        {
            tutorialStage = currentProgress
        };
    }

    public void load(NotificationsData data)
    {
        if(isOpen && data.tutorialStage > currentProgress)
        {
            closeStage();
        }
        currentProgress = data.tutorialStage;
    }

    public void clear()
    {
        currentProgress = 0;
    }

}
