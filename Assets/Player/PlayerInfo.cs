using Mirror;
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
        public MenuHandler.Menu? menuScreen;
        public float timer;
    }

    TutorialState tutorial = new TutorialState
    {
        stages = new List<TutorialWindow>()
        {
            //maps
            new TutorialWindow
            {
                start = new FireConditions
                {
                    menuScreen= MenuHandler.Menu.MainMenu
                },
                sections = new List<TutorialSection>()
                {
                    new TutorialSection
                    {
                        displayText = "Click On the Maps menu"
                    }
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
                    menuScreen = MenuHandler.Menu.Loading
                }
            },
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
                    }
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
                        displayText = "Reach the blue portal at the end!",
                    }
                },
                stop = new FinishCondtions
                {
                    timer = 10f
                }
            },
            //equip
            new TutorialWindow
            {
                start = new FireConditions
                {
                    menuScreen = MenuHandler.Menu.Loadout
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
                    timer = 30f
                }
            }
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

    private void OnDestroy()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        FindObjectOfType<MenuHandler>().MenuEvent -= OnOpenMenu;
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
            if (openWindow.stop.timer>0)
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

    void tryCloseStage()
    {
        if (!openWindow.stop.menuScreen.HasValue 
            && 
            (openWindow.stop.keyPress == null || openWindow.stop.keyPress.Count == 0)
            &&
            openWindow.stop.timer <= 0
            )
        {
            closeStage();
        }
    }

    void closeStage()
    {
        isOpen = false;
        currentProgress++;
        popups.closePopup();
    }

    void openStage()
    {
        isOpen = true;
        openWindow = tutorial.stages[currentProgress];
        popups.createTutorial(openWindow.sections);
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

}
