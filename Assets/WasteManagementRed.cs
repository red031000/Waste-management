using System.Collections.Generic;
using UnityEngine;
using WasteManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System;
using System.Collections;

public class WasteManagementRed : MonoBehaviour {
    #region Global Variables
    public KMBombInfo Info;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable BtnI, BtnV, BtnX, BtnL, Waste, Recycle, Submit, Reset;
    public TextMesh Screen;
    public GameObject BarControl;
    public MeshRenderer Bar;

    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;

    private int StartTime;
    private int CurrentTime;

    private List<string> ModulesName;

    private int Input = 0;

    private int PaperAmount = 0;
    private int PlasticAmount = 0;
    private int MetalAmount = 0;

    private int PaperRemaining = 0;
    private int PlasticRemaining = 0;
    private int MetalRemaining = 0;
    private int LeftoverRemaining = 0;

    private int PaperRecycle = 0;
    private int PaperRecycleAns = 0;
    private int PaperWaste = 0;
    private int PaperWasteAns = 0;

    private int PlasticRecycle = 0;
    private int PlasticRecycleAns = 0;
    private int PlasticWaste = 0;
    private int PlasticWasteAns = 0;

    private int MetalRecycle = 0;
    private int MetalRecycleAns = 0;
    private int MetalWaste = 0;
    private int MetalWasteAns = 0;

    private int LeftoverRecycle = 0;
    private int LeftoverRecycleAns = 0;
    private int LeftoverWaste = 0;
    private int LeftoverWasteAns = 0;

    private int ConsonantOccurances = 0;
    private int Stage = 1;

    private bool Morsemodules = false, Trnstrikes = false, Frkstrikes = false, Sigtime = false, Strike = false;

    private bool _isSolved = false, _lightsOn = false, Generated = false, Calculated = false, Barempty = false, ForcedSolve = false;

	#endregion

	#region Answer Calculation
	void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;
	}

    private void Awake() //button handlers
    {
        Reset.OnInteract += delegate ()
        {
            ResetHandler();
            return false;
        };
        Submit.OnInteract += delegate ()
        {
            SubmitHandler();
            return false;
        };
        Waste.OnInteract += delegate ()
        {
            WasteHandler();
            return false;
        };
        Recycle.OnInteract += delegate ()
        {
            RecycleHandler();
            return false;
        };
        BtnI.OnInteract += delegate ()
        {
            BtnIHandler();
            return false;
        };
        BtnV.OnInteract += delegate ()
        {
            BtnVHandler();
            return false;
        };
        BtnX.OnInteract += delegate ()
        {
            BtnXHandler();
            return false;
        };
        BtnL.OnInteract += delegate ()
        {
            BtnLHandler();
            return false;
        };
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }
        
    void Init()
    {
        if (!Generated)
        {
            StartTime = Mathf.FloorToInt(Info.GetTime());
            ModulesName = Info.GetModuleNames();
            ConsonantOccurances = Info.GetSerialNumber().Count("BCDFGHJKLMNPQRSTVWXYZ".Contains);
            GenerateAmounts(); //generate the initial amounts of paper, plastic and metal
            Screen.text = "Paper";
        }
        //reset
        if (Strike) UndoTime();
        Stage = 1;
        PaperRecycle = 0;
        PlasticRecycle = 0;
        MetalRecycle = 0;
        LeftoverRecycle = 0;
        Screen.text = "Paper";
        Screen.fontSize = 75;
        Calculated = false;

        PaperWaste = 0;
        PlasticWaste = 0;
        MetalWaste = 0;
        LeftoverWaste = 0;
        Input = 0;
        Strike = false;
    }

    void UndoTime()
    {
        if (Morsemodules)
        {
            PaperAmount += 26;
            Morsemodules = false;
        }
        if (Trnstrikes)
        {
            PlasticAmount -= 91;
            Trnstrikes = false;
        }
        if (Frkstrikes)
        {
            PlasticAmount -= 69;
            Frkstrikes = false;
        }
        if (Sigtime)
        {
            MetalAmount -= 99;
            Sigtime = false;
        }
    }

    void GenerateAmounts()
    {
        //paper
        if (Info.IsIndicatorPresent(Indicator.IND) && KMBombInfoExtensions.GetBatteryCount(Info) < 5)
        {
            PaperAmount += 19;
            Debug.LogFormat("[Waste Management #{0}] Added 19 to the paper amount (IND indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.SND))
        {
            PaperAmount += 15;
            Debug.LogFormat("[Waste Management #{0}] Added 15 to the paper amount (SND indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
        }
        if (Info.IsPortPresent(Port.Parallel))
        {
            PaperAmount -= 44;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 44 from the paper amount (parallel port)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
        }
        //don't calculate time dependent rules yet, as we don't know when the submit button is going to be pressed
        if (Info.GetBatteryCount() == 0 && Info.GetIndicators().Count() < 3)
        {
            PaperAmount += 154;
            Debug.LogFormat("[Waste Management #{0}] Added 154 to the paper amount (zero batteries)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
        }
        if (Info.GetSerialNumberLetters().Any("SAVEMYWORLD".Contains) && !(ConsonantOccurances > 2))
        {
            PaperAmount += 200;
            Debug.LogFormat("[Waste Management #{0}] Added 200 to the paper amount (Save My World)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
        }
        //plastic
        //miss all the check to do with strikes because we don't know how many strikes we have until we submit
        if (Info.GetPortPlates().Any(x => x.Length == 0) && ModulesName.Count % 2 == 0)
        {
            PlasticAmount -= 17;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 17 from the plastic amount (Empty port plate)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, PlasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.FRQ) && !(Info.GetBatteryCount(Battery.D) > Info.GetBatteryCount(Battery.AA)))
        {
            PlasticAmount += 153;
            Debug.LogFormat("[Waste Management #{0}] Added 153 to the plastic amount (FRQ indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, PlasticAmount);
        }
        //metal
        if (Info.IsIndicatorPresent(Indicator.BOB))
        {
            MetalAmount += 199;
            Debug.LogFormat("[Waste Management #{0}] Added 199 to the metal amount (BOB indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.MSA))
        {
            MetalAmount += 92;
            Debug.LogFormat("[Waste Management #{0}] Added 92 to the metal amount (MSA indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.CAR) && !(Info.IsPortPresent(Port.RJ45)))
        {
            MetalAmount -= 200;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 200 from the metal amount (CAR indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        }
        if (KMBombInfoExtensions.IsDuplicatePortPresent(Info) && !(Info.IsPortPresent(Port.DVI)))
        {
            MetalAmount += 153;
            Debug.LogFormat("[Waste Management #{0}] Added 153 to the metal amount (duplicate port)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        }
        //again, time stuff needs to be done a point of submission
        if (Info.IsIndicatorOn(Indicator.BOB) && Info.GetPortCount() >= 6 && ModulesName.Contains("Forget Me Not"))
        {
            MetalAmount += 99;
            Debug.LogFormat("[Waste Management #{0}] Added 99 to the metal amount (lit BOB and at least 6 ports)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        } else if (ModulesName.Contains("Forget Me Not"))
        {
            MetalAmount -= 84;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 84 from metal amount (Forget Me Not)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
        }
        //print temporary amounts. Perform the actual cleanup at submit time
        Debug.LogFormat("[Waste Management #{0}] The signed paper amount before time and strike based rules is {1}", _moduleId, PaperAmount);
        Debug.LogFormat("[Waste Management #{0}] The signed plastic amount before time and strike based rules is {1}", _moduleId, PlasticAmount);
        Debug.LogFormat("[Waste Management #{0}] The signed metal amount before time and strike based rules is {1}", _moduleId, MetalAmount);
        Generated = true;
    }

    private void TimeAdjustments()
    {
        if (ModulesName.Contains("Morse Code") || ModulesName.Contains("Morse-A-Maze") || ModulesName.Contains("Morsematics") || ModulesName.Contains("Color Morse") || ModulesName.Contains("Morse War") || ModulesName.Contains("Simon Sends"))
        {
            if (CurrentTime <= StartTime / 2)
            {
                Morsemodules = true;
                PaperAmount -= 26;
                Debug.LogFormat("[Waste Management #{0}] Taken 26 from the paper amount (Morse module)", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, PaperAmount);
            }
        }
        if (Info.IsIndicatorPresent(Indicator.TRN) && Info.GetStrikes() != 1)
        {
            Trnstrikes = true;
            PlasticAmount += 91;
            Debug.LogFormat("[Waste Management #{0}] Added 91 to the plastic amount (TRN indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, PlasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.FRK) && Info.GetStrikes() != 2)
        {
            Frkstrikes = true;
            PlasticAmount += 69;
            Debug.LogFormat("[Waste Management #{0}] Added 69 to the plastic amount (FRK indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, PlasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.SIG))
        {
            if (CurrentTime > StartTime / 5)
            {
                Sigtime = true;
                MetalAmount += 99;
                Debug.LogFormat("[Waste Management #{0}] Added 99 to the metal amount (SIG indicator)", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, MetalAmount);
            }
        }
        //cleanup again and print final after time and strike adjustments
        PaperRemaining =  Mathf.Abs(PaperAmount);
        PlasticRemaining =  Mathf.Abs(PlasticAmount);
        MetalRemaining = Mathf.Abs(MetalAmount);
        LeftoverRemaining = 0;
        Debug.LogFormat("[Waste Management #{0}] Final non-negative paper amount after time and strike based rules is {1}", _moduleId, PaperRemaining);
        Debug.LogFormat("[Waste Management #{0}] Final non-negative plastic amount after time and strike based rules is {1}", _moduleId, PlasticRemaining);
        Debug.LogFormat("[Waste Management #{0}] Final non-negative metal amount after time and strike based rules is {1}", _moduleId, MetalRemaining);
        Calculated = true;
    }

    private void CalculateProportions()
    {
        bool Continueto4 = false;
        bool Is4true = false;
        if (PaperRemaining + PlasticRemaining + MetalRemaining > 695)
        {
            PaperRecycleAns = PaperRemaining;
            PlasticRecycleAns = PlasticRemaining;
            MetalRecycleAns = MetalRemaining;
            PaperWasteAns = 0;
            PlasticWasteAns = 0;
            MetalWasteAns = 0;
            PaperRemaining = 0;
            PlasticRemaining = 0;
            MetalRemaining = 0;
            Debug.LogFormat("[Waste Management #{0}] Metal answer is recycle all", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic answer is recycle all", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle all", _moduleId);
        }
        else if (MetalRemaining > 200)
        {
            MetalRecycleAns = (int)(MetalRemaining * 0.75f);
            MetalWasteAns = (int)(MetalRemaining * 0.25f);
            MetalRemaining = 0;
            Debug.LogFormat("[Waste Management #{0}] Metal answer is recycle three quarters, waste one quarter", _moduleId);
            Continueto4 = true;
        }
        else if (MetalRemaining < PaperRemaining)
        {
            PaperRecycleAns = PaperRemaining;
            PaperRemaining = 0;
            MetalWasteAns = (int)(MetalRemaining * 0.25f);
            MetalRemaining = (int)(MetalRemaining * 0.75f);
            LeftoverRemaining = MetalRemaining + PlasticRemaining;
            LeftoverRecycleAns = (int)(LeftoverRemaining * 0.5f);
            Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle everything", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic answer is all to leftovers", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal answer is waste one quarter, the rest to leftovers", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Leftovers answer is recycle half", _moduleId);
        }
        else Continueto4 = true;
        if (Continueto4)
        {
            if (PlasticRemaining < 300 && PlasticRemaining > 100)
            {
                PlasticRecycleAns = (int)(PlasticRemaining * 0.5f);
                PlasticRemaining = (int)(PlasticRemaining * 0.5f);
                Is4true = true;
                Debug.LogFormat("[Waste Management #{0}] Plastic answer is recycle half", _moduleId);
            } else if (PlasticRemaining < 100 && PlasticRemaining > 10)
            {
                PlasticWasteAns = PlasticRemaining;
                PlasticRemaining = 0;
                Debug.LogFormat("[Waste Management #{0}] Plastic answer is waste all", _moduleId);
            }
            if (PaperRemaining < 65)
            {
                if (Is4true)
                {
                    PaperRecycleAns = PaperRemaining;
                    PaperRemaining = 0;
                    Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle all", _moduleId);
                } else
                {
                    PaperWasteAns = (int)(PaperRemaining / 3.0f);
                    PaperRemaining = (int)(2 * PaperRemaining / 3.0f);
                    Debug.LogFormat("[Waste Management #{0}] Paper answer is waste one third", _moduleId);
                }
            }
            LeftoverRemaining = PaperRemaining + PlasticRemaining + MetalRemaining;
            if (LeftoverRemaining < 300 && LeftoverRemaining > 100)
            {
                LeftoverRecycleAns = LeftoverRemaining;
                Debug.LogFormat("[Waste Management #{0}] Leftover answer is recycle all", _moduleId);
            } else
            {
                LeftoverWasteAns = LeftoverRemaining;
                Debug.LogFormat("[Waste Management #{0}] Leftover answer is waste all", _moduleId);
            }
        }
    }
	#endregion

	#region Button Handling
	private void BtnIHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnI.transform);
        BtnI.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty) //if the bar is empty
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        } else Input += 1;
    }

    private void BtnVHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnV.transform);
        BtnV.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else Input += 5;
    }

    private void BtnXHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnX.transform);
        BtnX.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else Input += 10;
    }

    private void BtnLHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BtnL.transform);
        BtnL.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else Input += 50;
    }

    private void RecycleHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Recycle.transform);
        Recycle.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (Stage == 1)
            PaperRecycle = Input;
        else if (Stage == 2)
            PlasticRecycle = Input;
        else if (Stage == 3)
            MetalRecycle = Input;
        else
            LeftoverRecycle = Input;
        Input = 0;
    }

    private void WasteHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Waste.transform);
        Waste.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (Stage == 1)
            PaperWaste = Input;
        else if (Stage == 2)
            PlasticWaste = Input;
        else if (Stage == 3)
            MetalWaste = Input;
        else
            LeftoverWaste = Input;
        Input = 0;
    }

    private void ResetHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Reset.transform);
        Reset.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (Barempty)
        {
            Module.HandleStrike();
            Strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (Stage == 1)
        {
            Input = 0;
            PaperWaste = 0;
            PaperRecycle = 0;
        } else if (Stage == 2)
        {
            Input = 0;
            PlasticWaste = 0;
            PlasticRecycle = 0;
        } else if (Stage == 3)
        {
            Input = 0;
            MetalWaste = 0;
            MetalRecycle = 0;
        } else
        {
            Input = 0;
            LeftoverWaste = 0;
            LeftoverRecycle = 0;
        }
    }

    private void SubmitHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Submit.transform);
        Submit.AddInteractionPunch();
        if (Barempty)
        {
            Barempty = false;
            BarControl.gameObject.transform.localScale = new Vector3(1, 1, 1);
            Debug.LogFormat("[Waste Management #{0}] Strike avoided, continue with next stage", _moduleId);
            return;
        }

        if (!_lightsOn || _isSolved) return;

        if (!Calculated && !ForcedSolve)
        {
            //once submit button is pressed for the first time, those rules will be in effect for the rest of the bomb, unless you get a strike on this module
            CurrentTime = Mathf.FloorToInt(Info.GetTime());
            Debug.LogFormat("[Waste Management #{0}] Submit button pressed, performing final adjustments", _moduleId);
            TimeAdjustments();
        }
        if (Stage == 1)
        {
            if (!ForcedSolve) CalculateProportions();
            Debug.LogFormat("[Waste Management #{0}] Received {1} for paper recycling, expected {2}", _moduleId, PaperRecycle, PaperRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Received {1} for paper waste, expected {2}", _moduleId, PaperWaste, PaperWasteAns);
            if (PaperRecycle == PaperRecycleAns && PaperWaste == PaperWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Paper correct!", _moduleId);
                Stage++;
                Input = 0;
                Screen.text = "Plastic";
                Screen.fontSize = 70;
            } else
            {
                Debug.LogFormat("[Waste Management #{0}] Paper incorrect, Strike.", _moduleId);
                Module.HandleStrike();
                Strike = true;
                Init();
            }
        } else if (Stage == 2)
        {
            Debug.LogFormat("[Waste Management #{0}] Received {1} for plastic recycling, expected {2}", _moduleId, PlasticRecycle, PlasticRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Received {1} for plastic waste, expected {2}", _moduleId, PlasticWaste, PlasticWasteAns);
            if (PlasticRecycle == PlasticRecycleAns && PlasticWaste == PlasticWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Plastic correct!", _moduleId);
                Stage++;
                Input = 0;
                Screen.text = "Metal";
                Screen.fontSize = 75;
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Plastic incorrect, Strike.", _moduleId);
                Module.HandleStrike();
                Strike = true;
                Init();
            }
        } else if (Stage == 3)
        {
            Debug.LogFormat("[Waste Management #{0}] Received {1} for metal recycling, expected {2}", _moduleId, MetalRecycle, MetalRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Received {1} for metal waste, expected {2}", _moduleId, MetalWaste, MetalWasteAns);
            if (MetalRecycle == MetalRecycleAns && MetalWaste == MetalWasteAns)
            {
                if (LeftoverRecycleAns > 0 || LeftoverWasteAns > 0)
                {
                    Debug.LogFormat("[Waste Management #{0}] Metal correct!", _moduleId);
                    Stage++;
                    Input = 0;
                    Screen.text = "Leftovers";
                    Screen.fontSize = 50;
                } else
                {
                    Debug.LogFormat("[Waste Management #{0}] Metal correct!", _moduleId);
                    Debug.LogFormat("[Waste Management #{0}] There are no leftovers", _moduleId);
                    Debug.LogFormat("[Waste Management #{0}] Module Passed.", _moduleId);
                    _isSolved = true; //module is solved
                    Module.HandlePass();
                    Audio.PlaySoundAtTransform("wastemana", Submit.transform);
                    Input = 0;
                    Screen.text = "";
                    Screen.fontSize = 75;
                }
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Metal incorrect, Strike.", _moduleId);
                Strike = true;
                Module.HandleStrike();
                Init();
            }
        } else
        {
            Debug.LogFormat("[Waste Management #{0}] Received {1} for leftover recycling, expected {2}", _moduleId, LeftoverRecycle, LeftoverRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Received {1} for leftover waste, expected {2}", _moduleId, LeftoverWaste, LeftoverWasteAns);
            if (LeftoverRecycle == LeftoverRecycleAns && LeftoverWaste == LeftoverWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Leftovers correct!", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Module Passed.", _moduleId);
                _isSolved = true; //module is solved
                Module.HandlePass();
                Audio.PlaySoundAtTransform("wastemana", Submit.transform);
                Input = 0;
                Screen.text = "";
                Screen.fontSize = 75;
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Leftovers incorrect, Strike.", _moduleId);
                Strike = true;
                Module.HandleStrike();
                Init();
            }
        }
        if (Stage >= 1 && Stage < 4)
        {
            int random = UnityEngine.Random.Range(1, 21); //5% chance of bar going blank
            if (random == 1)
            {
                Barempty = true;
                BarControl.gameObject.transform.localScale = new Vector3(1, 1, 0);
                Debug.LogFormat("[Waste Management #{0}] Bar empty, submit expected or strike", _moduleId);
            }
        }
    }
    #endregion

    #region Twitch Plays
    //twitch plays commands
#pragma warning disable 414
    private string TwitchHelpMessage = "Allocate the number 66 to waste with !{0} LXVIW. Change the W to an R for recycling. Reset the module with !{0} Reset. Submit the answer with !{0} Submit.";
    private string TwitchManualCode = "Waste Management";
#pragma warning restore 414
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (command.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Reset };
        }
        else if (command.Equals("submit", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { Submit };
        }
        else if (Regex.IsMatch(command, @"^[lxvi]+[wr]?$"))
        {
            KMSelectable[] totalselect = { };
            foreach (char c in command)
            {
                if (c == 'i')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { BtnI }).ToArray();
                }
                else if (c == 'v')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { BtnV }).ToArray();
                }
                else if (c == 'x')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { BtnX }).ToArray();
                }
                else if (c == 'l')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { BtnL }).ToArray();
                }
                else if (c == 'w')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { Waste }).ToArray();
                }
                else if (c == 'r')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { Recycle }).ToArray();
                }
            }
            return totalselect;
        } else if (Regex.IsMatch(command, @"^[wr]$"))
        {
            if (command == "w")
            {
                return new KMSelectable[] { Waste };
            } else
            {
                return new KMSelectable[] { Recycle };
            }
        }
        else
            return null;
    }
	private IEnumerator TwitchHandleForcedSolve()
	{
		if (!_isSolved)
		{
			yield return null;
			ForcedSolve = true;
			TimeAdjustments();
			CalculateProportions();
			Debug.LogFormat("[Waste Management #{0}] Forced solve.", _moduleId);
			while (!_isSolved)
			{
				if (Barempty == true) SubmitHandler();
				if (Stage == 1)
				{
					int PaperTempAns = PaperWasteAns;
					int PaperLPress = PaperTempAns / 50;
					PaperTempAns %= 50;
					int PaperXPress = PaperTempAns / 10;
					PaperTempAns %= 10;
					int PaperVPress = PaperTempAns / 5;
					PaperTempAns %= 5;
					int PaperIPress = PaperTempAns;

					for (int i = 0; i < PaperLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					WasteHandler();
					yield return new WaitForSeconds(0.1f);

					PaperTempAns = PaperRecycleAns;
					PaperLPress = PaperTempAns / 50;
					PaperTempAns %= 50;
					PaperXPress = PaperTempAns / 10;
					PaperTempAns %= 10;
					PaperVPress = PaperTempAns / 5;
					PaperTempAns %= 5;
					PaperIPress = PaperTempAns;

					for (int i = 0; i < PaperLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PaperIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					RecycleHandler();
					yield return new WaitForSeconds(0.1f);

					SubmitHandler();
					yield return new WaitForSeconds(0.1f);
				} else if (Stage == 2)
				{
					int PlasticTempAns = PlasticWasteAns;
					int PlasticLPress = PlasticTempAns / 50;
					PlasticTempAns %= 50;
					int PlasticXPress = PlasticTempAns / 10;
					PlasticTempAns %= 10;
					int PlasticVPress = PlasticTempAns / 5;
					PlasticTempAns %= 5;
					int PlasticIPress = PlasticTempAns;

					for (int i = 0; i < PlasticLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					WasteHandler();
					yield return new WaitForSeconds(0.1f);

					PlasticTempAns = PlasticRecycleAns;
					PlasticLPress = PlasticTempAns / 50;
					PlasticTempAns %= 50;
					PlasticXPress = PlasticTempAns / 10;
					PlasticTempAns %= 10;
					PlasticVPress = PlasticTempAns / 5;
					PlasticTempAns %= 5;
					PlasticIPress = PlasticTempAns;

					for (int i = 0; i < PlasticLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < PlasticIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					RecycleHandler();
					yield return new WaitForSeconds(0.1f);

					SubmitHandler();
					yield return new WaitForSeconds(0.1f);
				} else if (Stage == 3)
				{
					int MetalTempAns = MetalWasteAns;
					int MetalLPress = MetalTempAns / 50;
					MetalTempAns %= 50;
					int MetalXPress = MetalTempAns / 10;
					MetalTempAns %= 10;
					int MetalVPress = MetalTempAns / 5;
					MetalTempAns %= 5;
					int MetalIPress = MetalTempAns;

					for (int i = 0; i < MetalLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					WasteHandler();
					yield return new WaitForSeconds(0.1f);

					MetalTempAns = MetalRecycleAns;
					MetalLPress = MetalTempAns / 50;
					MetalTempAns %= 50;
					MetalXPress = MetalTempAns / 10;
					MetalTempAns %= 10;
					MetalVPress = MetalTempAns / 5;
					MetalTempAns %= 5;
					MetalIPress = MetalTempAns;

					for (int i = 0; i < MetalLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < MetalIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					RecycleHandler();
					yield return new WaitForSeconds(0.1f);

					SubmitHandler();
					yield return new WaitForSeconds(0.1f);
				} else if (Stage == 4)
				{
					int LeftoverTempAns = LeftoverWasteAns;
					int LeftoverLPress = LeftoverTempAns / 50;
					LeftoverTempAns %= 50;
					int LeftoverXPress = LeftoverTempAns / 10;
					LeftoverTempAns %= 10;
					int LeftoverVPress = LeftoverTempAns / 5;
					LeftoverTempAns %= 5;
					int LeftoverIPress = LeftoverTempAns;

					for (int i = 0; i < LeftoverLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					WasteHandler();
					yield return new WaitForSeconds(0.1f);

					LeftoverTempAns = LeftoverRecycleAns;
					LeftoverLPress = LeftoverTempAns / 50;
					LeftoverTempAns %= 50;
					LeftoverXPress = LeftoverTempAns / 10;
					LeftoverTempAns %= 10;
					LeftoverVPress = LeftoverTempAns / 5;
					LeftoverTempAns %= 5;
					LeftoverIPress = LeftoverTempAns;

					for (int i = 0; i < LeftoverLPress; i++)
					{
						BtnLHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverXPress; i++)
					{
						BtnXHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverVPress; i++)
					{
						BtnVHandler();
						yield return new WaitForSeconds(0.1f);
					}
					for (int i = 0; i < LeftoverIPress; i++)
					{
						BtnIHandler();
						yield return new WaitForSeconds(0.1f);
					}

					RecycleHandler();
					yield return new WaitForSeconds(0.1f);

					SubmitHandler();
					yield return new WaitForSeconds(0.1f);
				}
			}
		}
	}
    #endregion
}
