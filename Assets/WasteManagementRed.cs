using System.Collections.Generic;
using UnityEngine;
using WasteManagement;
using System.Linq;
using System.Text.RegularExpressions;
using System;

public class WasteManagementRed : MonoBehaviour {
    #region GlobalVariables
    public KMBombInfo Info;
    public KMAudio Audio;
    public KMBombModule Module;

    public KMSelectable btnI, btnV, btnX, btnL, Waste, Recycle, submit, reset;
    public TextMesh screen;
    public GameObject barControl;
    public MeshRenderer bar;

    private static int _moduleIdCounter = 1;
    private int _moduleId = 0;

    private int startTime;
    private int currentTime;

    private List<string> modulesName;

    private int input = 0;

    private int paperAmount = 0;
    private int plasticAmount = 0;
    private int metalAmount = 0;

    private int paperRemaining = 0;
    private int plasticRemaining = 0;
    private int metalRemaining = 0;
    private int leftoverRemaining = 0;

    private int paperRecycle = 0;
    private int paperRecycleAns = 0;
    private int paperWaste = 0;
    private int paperWasteAns = 0;

    private int plasticRecycle = 0;
    private int plasticRecycleAns = 0;
    private int plasticWaste = 0;
    private int plasticWasteAns = 0;

    private int metalRecycle = 0;
    private int metalRecycleAns = 0;
    private int metalWaste = 0;
    private int metalWasteAns = 0;

    private int leftoverRecycle = 0;
    private int leftoverRecycleAns = 0;
    private int leftoverWaste = 0;
    private int leftoverWasteAns = 0;

    private int consonantOccurances = 0;
    private int stage = 1;

    private bool morsemodules = false, trnstrikes = false, frkstrikes = false, sigtime = false, strike = false;

    private bool _isSolved = false, _lightsOn = false, generated = false, calculated = false, barempty = false;

    #endregion

    void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;
	}

    private void Awake() //button handlers
    {
        reset.OnInteract += delegate ()
        {
            resetHandler();
            return false;
        };
        submit.OnInteract += delegate ()
        {
            submitHandler();
            return false;
        };
        Waste.OnInteract += delegate ()
        {
            wasteHandler();
            return false;
        };
        Recycle.OnInteract += delegate ()
        {
            recycleHandler();
            return false;
        };
        btnI.OnInteract += delegate ()
        {
            btnIHandler();
            return false;
        };
        btnV.OnInteract += delegate ()
        {
            btnVHandler();
            return false;
        };
        btnX.OnInteract += delegate ()
        {
            btnXHandler();
            return false;
        };
        btnL.OnInteract += delegate ()
        {
            btnLHandler();
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
        if (!generated)
        {
            startTime = Mathf.FloorToInt(Info.GetTime());
            modulesName = Info.GetModuleNames();
            consonantOccurances = Info.GetSerialNumber().Count("BCDFGHJKLMNPQRSTVWXYZ".Contains); //double oops
            generateAmounts(); //generate the initial amounts of paper, plastic and metal
            screen.text = "Paper";
        }
        //reset
        if (strike) UndoTime();
        stage = 1;
        paperRecycle = 0;
        plasticRecycle = 0;
        metalRecycle = 0;
        leftoverRecycle = 0;
        screen.text = "Paper";
        screen.fontSize = 75;
        calculated = false;

        paperWaste = 0;
        plasticWaste = 0;
        metalWaste = 0;
        leftoverWaste = 0;
        input = 0;
        strike = false;
    }

    void UndoTime()
    {
        if (morsemodules)
        {
            paperAmount += 26;
            morsemodules = false;
        }
        if (trnstrikes)
        {
            plasticAmount -= 91;
            trnstrikes = false;
        }
        if (frkstrikes)
        {
            plasticAmount -= 69;
            frkstrikes = false;
        }
        if (sigtime)
        {
            metalAmount -= 99;
            sigtime = false;
        }
    }

    void generateAmounts()
    {
        //paper
        if (Info.IsIndicatorPresent(Indicator.IND) && KMBombInfoExtensions.GetBatteryCount(Info) < 5)
        {
            paperAmount += 19;
            Debug.LogFormat("[Waste Management #{0}] Added 19 to the paper amount (ind indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.SND))
        {
            paperAmount += 15;
            Debug.LogFormat("[Waste Management #{0}] Added 15 to the paper amount (snd indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
        }
        if (Info.IsPortPresent(Port.Parallel))
        {
            paperAmount -= 44;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 44 from the paper amount (parallel port)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
        }
        //don't calculate time dependent rules yet, as we don't know when the submit button is going to be pressed
        if (Info.GetBatteryCount() == 0 && Info.GetIndicators().Count() < 3) //again, oops
        {
            paperAmount += 154;
            Debug.LogFormat("[Waste Management #{0}] Added 154 to the paper amount (zero batteries)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
        }
        if (Info.GetSerialNumberLetters().Any("SAVEMYWORLD".Contains) && !(consonantOccurances > 2)) //oops
        {
            paperAmount += 200;
            Debug.LogFormat("[Waste Management #{0}] Added 200 to the paper amount (Save My World)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
        }
        //plastic
        //miss all the check to do with strikes because we don't know how many strikes we have untill we submit
        if (Info.GetPortPlates().Any(x => x.Length == 0) && modulesName.Count % 2 == 0)
        {
            plasticAmount -= 17;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 17 from the plastic amount (Empty port plate)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, plasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.FRQ) && Info.GetBatteryCount(Battery.D) < Info.GetBatteryCount(Battery.AA))
        {
            plasticAmount += 153;
            Debug.LogFormat("[Waste Management #{0}] Added 153 to the plastic amount (frq indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, plasticAmount);
        }
        //metal
        if (Info.IsIndicatorPresent(Indicator.BOB))
        {
            metalAmount += 199;
            Debug.LogFormat("[Waste Management #{0}] Added 199 to the metal amount (bob indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.MSA))
        {
            metalAmount += 92;
            Debug.LogFormat("[Waste Management #{0}] Added 92 to the metal amount (msa indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.CAR) && !(Info.IsPortPresent(Port.RJ45)))
        {
            metalAmount -= 200;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 200 from the metal amount (car indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        }
        if (KMBombInfoExtensions.IsDuplicatePortPresent(Info) && !(Info.IsPortPresent(Port.DVI)))
        {
            metalAmount += 153;
            Debug.LogFormat("[Waste Management #{0}] Added 153 to the metal amount (duplicate port)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        }
        //again, time stuff needs to be done a point of submission
        if (Info.IsIndicatorOn(Indicator.BOB) && Info.GetPortCount() >= 6)
        {
            metalAmount += 99;
            Debug.LogFormat("[Waste Management #{0}] Added 99 to the metal amount (lit bob and at least 6 ports)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        } else if (modulesName.Contains("Forget Me Not"))
        {
            metalAmount -= 84;
            Debug.LogFormat("[Waste Management #{0}] Subtracted 84 from metal amount (Forget Me Not)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
        }
        //print final amounts and cleanup
        if (paperAmount < -1)
        {
            paperAmount *= -1;
        }
        if (plasticAmount < -1)
        {
            plasticAmount *= -1;
        }
        if (metalAmount < -1)
        {
            metalAmount *= -1;
        }
        Debug.LogFormat("[Waste Management #{0}] Final paper amount before time and strike based rules is {1}", _moduleId, paperAmount);
        Debug.LogFormat("[Waste Management #{0}] Final plastic amount before time and strike based rules is {1}", _moduleId, plasticAmount);
        Debug.LogFormat("[Waste Management #{0}] Final metal amount before time and strike based rules is {1}", _moduleId, metalAmount);
        generated = true;
    }

    private void timeAdjustments()
    {
        if (modulesName.Contains("Morse Code") || modulesName.Contains("Morse-A-Maze") || modulesName.Contains("Morsematics") || modulesName.Contains("Color Morse"))
        {
            if (currentTime <= startTime / 2)
            {
                morsemodules = true;
                paperAmount -= 26;
                Debug.LogFormat("[Waste Management #{0}] Taken 26 from the paper amount (Morse module)", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Paper amount is now {1}", _moduleId, paperAmount);
            }
        }
        if (Info.IsIndicatorPresent(Indicator.TRN) && Info.GetStrikes() != 1)
        {
            trnstrikes = true;
            plasticAmount += 91;
            Debug.LogFormat("[Waste Management #{0}] Added 91 to the plastic amount (trn indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, plasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.FRK) && Info.GetStrikes() != 2)
        {
            frkstrikes = true;
            plasticAmount += 69;
            Debug.LogFormat("[Waste Management #{0}] Added 69 to the plastic amount (frk indicator)", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic amount is now {1}", _moduleId, plasticAmount);
        }
        if (Info.IsIndicatorPresent(Indicator.SIG))
        {
            if (currentTime > startTime / 5)
            {
                sigtime = true;
                metalAmount += 99;
                Debug.LogFormat("[Waste Management #{0}] Added 99 to the metal amount (sig indicator)", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Metal amount is now {1}", _moduleId, metalAmount);
            }
        }
        //cleanup again and print final after time and strike adjustments
        if (paperAmount < -1)
        {
            paperAmount *= -1;
        }
        if (plasticAmount < -1)
        {
            plasticAmount *= -1;
        }
        if (metalAmount < -1)
        {
            metalAmount *= -1;
        }
        Debug.LogFormat("[Waste Management #{0}] Final paper amount after time and strike based rules is {1}", _moduleId, paperAmount);
        Debug.LogFormat("[Waste Management #{0}] Final plastic amount after time and strike based rules is {1}", _moduleId, plasticAmount);
        Debug.LogFormat("[Waste Management #{0}] Final metal amount after time and strike based rules is {1}", _moduleId, metalAmount);
        calculated = true;
    }

    private void calculateProportions()
    {
        bool continueto4 = false;
        bool is4true = false;
        if (paperRemaining + plasticRemaining + metalRemaining > 695)
        {
            paperRecycleAns = paperRemaining;
            plasticRecycleAns = plasticRemaining;
            metalRecycleAns = metalRemaining;
            paperWasteAns = 0;
            plasticWasteAns = 0;
            metalWasteAns = 0;
            paperRemaining = 0;
            plasticRemaining = 0;
            metalRemaining = 0;
            Debug.LogFormat("[Waste Management #{0}] Metal answer is recycle all", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic answer is recycle all", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle all", _moduleId);
        }
        else if (metalRemaining > 200)
        {
            metalRecycleAns = (int)(metalRemaining * 0.75f);
            metalWasteAns = (int)(metalRemaining * 0.25f);
            metalRemaining = 0;
            Debug.LogFormat("[Waste Management #{0}] Metal answer is recycle three quarters, waste one quarter", _moduleId);
            continueto4 = true;
        }
        else if (metalRemaining < paperRemaining)
        {
            paperRecycleAns = paperRemaining;
            paperRemaining = 0;
            metalWasteAns = (int)(metalRemaining * 0.25f);
            metalRemaining = (int)(metalRemaining * 0.75f);
            leftoverRemaining = metalRemaining + plasticRemaining;
            leftoverRecycleAns = (int)(leftoverRemaining * 0.5f);
            Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle everything", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Plastic answer is all to leftovers", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Metal answer is waste one quarter, the rest to leftovers", _moduleId);
            Debug.LogFormat("[Waste Management #{0}] Leftovers answer is recycle half", _moduleId);
        }
        else continueto4 = true;
        if (continueto4)
        {
            if (plasticRemaining < 300 && plasticRemaining > 100)
            {
                plasticRecycleAns = (int)(plasticRemaining * 0.5f);
                plasticRemaining = (int)(plasticRemaining * 0.5f);
                is4true = true;
                Debug.LogFormat("[Waste Management #{0}] Plastic answer is recycle half", _moduleId);
            } else if (plasticRemaining < 100 && plasticRemaining > 10)
            {
                plasticWasteAns = plasticRemaining;
                plasticRemaining = 0;
                Debug.LogFormat("[Waste Management #{0}] Plastic answer is waste all", _moduleId);
            }
            if (paperRemaining < 65)
            {
                if (is4true)
                {
                    paperRecycleAns = paperRemaining;
                    paperRemaining = 0;
                    Debug.LogFormat("[Waste Management #{0}] Paper answer is recycle all", _moduleId);
                } else
                {
                    paperWasteAns = (int)(paperRemaining * (1/3));
                    paperRemaining = (int)(paperRemaining * (2/3));
                    Debug.LogFormat("[Waste Management #{0}] Paper answer is waste one third", _moduleId);
                }
            }
            leftoverRemaining = paperRemaining + plasticRemaining + metalRemaining;
            if (leftoverRemaining < 300 && leftoverRemaining > 100)
            {
                leftoverRecycleAns = leftoverRemaining;
                Debug.LogFormat("[Waste Management #{0}] Leftover answer is recycle all", _moduleId);
            } else
            {
                leftoverWasteAns = leftoverRemaining;
                Debug.LogFormat("[Waste Management #{0}] Leftover answer is waste all", _moduleId);
            }
        }
    }
    #region ButtonHandling
    private void btnIHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnI.transform);
        btnI.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty) //if the bar is empty
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        } else input += 1;
    }

    private void btnVHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnV.transform);
        btnV.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else input += 5;
    }

    private void btnXHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnX.transform);
        btnX.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else input += 10;
    }

    private void btnLHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, btnL.transform);
        btnL.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        else input += 50;
    }

    private void recycleHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Recycle.transform);
        Recycle.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (stage == 1)
            paperRecycle = input;
        else if (stage == 2)
            plasticRecycle = input;
        else if (stage == 3)
            metalRecycle = input;
        else
            leftoverRecycle = input;
        input = 0;
    }

    private void wasteHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Waste.transform);
        Waste.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (stage == 1)
            paperWaste = input;
        else if (stage == 2)
            plasticWaste = input;
        else if (stage == 3)
            metalWaste = input;
        else
            leftoverWaste = input;
        input = 0;
    }

    private void resetHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, reset.transform);
        reset.AddInteractionPunch();
        if (!_lightsOn || _isSolved) return;
        if (barempty)
        {
            Module.HandleStrike();
            strike = true;
            Debug.LogFormat("[Waste Management #{0}] Strike given, reset the module", _moduleId);
            Init();
        }
        if (stage == 1)
        {
            input = 0;
            paperWaste = 0;
            paperRecycle = 0;
        } else if (stage == 2)
        {
            input = 0;
            plasticWaste = 0;
            plasticRecycle = 0;
        } else if (stage == 3)
        {
            input = 0;
            metalWaste = 0;
            metalRecycle = 0;
        } else
        {
            input = 0;
            leftoverWaste = 0;
            leftoverRecycle = 0;
        }
    }

    private void submitHandler()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
        submit.AddInteractionPunch();
        if (barempty)
        {
            barempty = false;
            barControl.gameObject.transform.localScale = new Vector3(1, 1, 1);
            Debug.LogFormat("[Waste Management #{0}] Strike avoided, continue with next stage", _moduleId);
            return;
        }

        if (!_lightsOn || _isSolved) return;

        if (!calculated)
        {
            //once submit button is pressed for the first time, those rules will be in effect for the rest of the bomb, unless you get a strike on this module
            currentTime = Mathf.FloorToInt(Info.GetTime());
            Debug.LogFormat("[Waste Management #{0}] Submit button pressed, performing final adjustments", _moduleId);
            timeAdjustments();
        }
        if (stage == 1)
        {
            metalRemaining = metalAmount;
            plasticRemaining = plasticAmount;
            paperRemaining = paperAmount;
            leftoverRemaining = 0;
            calculateProportions();
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for paper recycling, expected {2}", _moduleId, paperRecycle, paperRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for paper waste, expected {2}", _moduleId, paperWaste, paperWasteAns);
            if (paperRecycle == paperRecycleAns && paperWaste == paperWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Paper correct!", _moduleId);
                stage++;
                input = 0;
                screen.text = "Plastic";
                screen.fontSize = 70;
            } else
            {
                Debug.LogFormat("[Waste Management #{0}] Paper incorrect, Strike.", _moduleId);
                Module.HandleStrike();
                strike = true;
                Init();
            }
        } else if (stage == 2)
        {
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for plastic recycling, expected {2}", _moduleId, plasticRecycle, plasticRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for plastic waste, expected {2}", _moduleId, plasticWaste, plasticWasteAns);
            if (plasticRecycle == plasticRecycleAns && plasticWaste == plasticWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Plastic correct!", _moduleId);
                stage++;
                input = 0;
                screen.text = "Metal";
                screen.fontSize = 75;
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Plastic incorrect, Strike.", _moduleId);
                Module.HandleStrike();
                strike = true;
                Init();
            }
        } else if (stage == 3)
        {
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for metal recycling, expected {2}", _moduleId, metalRecycle, metalRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for metal waste, expected {2}", _moduleId, metalWaste, metalWasteAns);
            if (metalRecycle == metalRecycleAns && metalWaste == metalWasteAns)
            {
                if (leftoverRecycleAns > 0 || leftoverWasteAns > 0)
                {
                    Debug.LogFormat("[Waste Management #{0}] Metal correct!", _moduleId);
                    stage++;
                    input = 0;
                    screen.text = "Leftovers";
                    screen.fontSize = 50;
                } else
                {
                    Debug.LogFormat("[Waste Management #{0}] Metal correct!", _moduleId);
                    Debug.LogFormat("[Waste Management #{0}] There are no leftovers", _moduleId);
                    Debug.LogFormat("[Waste Management #{0}] Module Passed.", _moduleId);
                    _isSolved = true; //module is solved
                    Module.HandlePass();
                    Audio.PlaySoundAtTransform("wastemana", submit.transform);
                    input = 0;
                    screen.text = "";
                    screen.fontSize = 75;
                }
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Metal incorrect, Strike.", _moduleId);
                strike = true;
                Module.HandleStrike();
                Init();
            }
        } else
        {
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for leftover recycling, expected {2}", _moduleId, leftoverRecycle, leftoverRecycleAns);
            Debug.LogFormat("[Waste Management #{0}] Recieved {1} for leftover waste, expected {2}", _moduleId, leftoverWaste, leftoverWasteAns);
            if (leftoverRecycle == leftoverRecycleAns && leftoverWaste == leftoverWasteAns)
            {
                Debug.LogFormat("[Waste Management #{0}] Leftovers correct!", _moduleId);
                Debug.LogFormat("[Waste Management #{0}] Module Passed.", _moduleId);
                _isSolved = true; //module is solved
                Module.HandlePass();
                Audio.PlaySoundAtTransform("wastemana", submit.transform);
                input = 0;
                screen.text = "";
                screen.fontSize = 75;
            }
            else
            {
                Debug.LogFormat("[Waste Management #{0}] Leftovers incorrect, Strike.", _moduleId);
                strike = true;
                Module.HandleStrike();
                Init();
            }
        }
        if (stage >= 1 && stage < 4)
        {
            int random = UnityEngine.Random.Range(1, 21); //5% chance of bar going blank
            if (random == 1)
            {
                barempty = true;
                barControl.gameObject.transform.localScale = new Vector3(1, 1, 0);
                Debug.LogFormat("[Waste Management #{0}] Bar empty, submit expected or strike", _moduleId);
            }
        }
    }
    #endregion
    #region TwitchPlays
    //twitch plays commands
    public string TwitchHelpMessage = "Allocate the number 66 to waste with !{0} LXVIW. Change the W to an R for recycling. Reset the module with !{0} Reset. Submit the answer with !{0} Submit.";
    public string TwitchManualCode = "https://ktane.timwi.de/HTML/Waste%20Management.html";
    public KMSelectable[] ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant().Trim();
        if (command.Equals("reset", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { reset };
        }
        else if (command.Equals("submit", StringComparison.InvariantCultureIgnoreCase))
        {
            return new KMSelectable[] { submit };
        }
        else if (Regex.IsMatch(command, @"^[lxvi]+[wr]?$"))
        {
            KMSelectable[] totalselect = { };
            foreach (char c in command)
            {
                if (c == 'i')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { btnI }).ToArray();
                }
                else if (c == 'v')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { btnV }).ToArray();
                }
                else if (c == 'x')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { btnX }).ToArray();
                }
                else if (c == 'l')
                {
                    totalselect = totalselect.Concat(new KMSelectable[] { btnL }).ToArray();
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
    #endregion
}