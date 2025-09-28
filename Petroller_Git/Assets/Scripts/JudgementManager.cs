using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.Samples;
using TMPro;
using Unity.VisualScripting;

// using Oculus.Interaction.Samples;
using UnityEngine;
using UnityEngine.UI;
using static OVRVirtualKeyboard;

public class JudgementManager : MonoBehaviour
{
    public static JudgementManager Instance;
    private bool isCooldown = false;
    private bool gameStarted = false;
    private float remainingCooldownTime;
    [SerializeField] float cooldownTime;
    private int[] inputCounts = { 0, 0, 0, 0, 0, 0 };

    private int[] judgementResults = { 0, 0, 0, 0, 0 }; // Perfect Great Good Bad Miss
    [SerializeField] private int commandAmount;
    private int currentCommandAmount = 0;
    private enum Judgements { Perfect, Great, Good, Bad, Miss }
    [Header("Judgement setting")]
    [SerializeField] private float[] judgementTiming = { 4f, 3f, 1f, 0f }; // Perfect Great Good Bad
    private float remainingjudgementTime = 0f;
    [SerializeField] float judgementTime = 5f;
    [Header("UI object")]
    [SerializeField] GameObject judgementUI;
    [SerializeField] GameObject resultUI;
    [SerializeField] GameObject startHint;
    [SerializeField] Image judgementTimer;
    [SerializeField] TextMeshProUGUI judgementResultDisplay;
    [SerializeField] TextMeshProUGUI resultDisplay;
    [SerializeField] TextMeshProUGUI countDisplay;
    public enum InputCommands { JoystickUp, JoystickDown, JoystickLeft, JoystickRight, Trigger, Grip }

    private string currentCommand = "";
    private int commandIndex = 0;
    [SerializeField] Animator commandAnimator;
    private float timer = 0f;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddPress(string inputSource)
    {
        //Debug.Log("Input Command");
        if (isCooldown) return;
        if (!gameStarted)
        {
            startGame();
            return;
        }
        inputCounts[(int)Enum.Parse(typeof(InputCommands), inputSource)]++;
        if (inputSource == currentCommand)
        {
            for (int i = 0; i < judgementTiming.Length; i++)
            {
                if (remainingjudgementTime >= judgementTiming[i])
                {
                    judgementResultDisplay.text = Enum.GetName(typeof(Judgements), i);
                    Invoke("hideJudgementResult", 1f);
                    judgementResults[i]++;
                    remainingjudgementTime = 0f;
                    return;
                }
            }
        }
        if (currentCommand == "Stop")
        {
            judgementResultDisplay.text = "Miss";
            judgementResults[4]++;
        }
        remainingjudgementTime = 0f;
        return;
    }
    private void hideJudgementResult() {
        judgementResultDisplay.text = "";
    }
    private void nextCommand()
    {
        //Debug.Log("Next Command");
        int tmp;
        do
        {
            tmp = UnityEngine.Random.Range(0, 6);
        } while (commandIndex == tmp);
        commandIndex = tmp;
        currentCommand = Enum.GetName(typeof(InputCommands), commandIndex);
        commandAnimator.SetInteger("index", commandIndex);
        StartCoroutine(JudgementCoroutine());
    }

    private IEnumerator JudgementCoroutine()
    {
        remainingjudgementTime = judgementTime;
        while (remainingjudgementTime > 0)
        {
            remainingjudgementTime -= Time.deltaTime;
            judgementTimer.fillAmount = remainingjudgementTime / judgementTime;
            yield return null;
        }
        //Debug.Log("Time Up");
        
        if (judgementResultDisplay.text == "")
        {
            judgementResultDisplay.text = "Miss";
            judgementResults[4]++;
        }
        remainingjudgementTime = 0f;
        currentCommandAmount = 0;
        for (int i = 0; i < judgementResults.Length; i++)
        {
            currentCommandAmount += judgementResults[i];
        }
        countDisplay.text = $"{currentCommandAmount} / {commandAmount}";
        Invoke("hideJudgementResult", 1f);
        StartCoroutine(CooldownCoroutine());
        yield break;
    }
    private IEnumerator CooldownCoroutine()
    {
        isCooldown = true;
        remainingCooldownTime = cooldownTime;
        commandAnimator.SetInteger("index", 6);
        //Debug.Log("Cool Down");
        while (remainingCooldownTime > 0f)
        {
            remainingCooldownTime -= Time.deltaTime;
            judgementTimer.fillAmount = 1 - remainingCooldownTime / cooldownTime;
            yield return null;
        }

        isCooldown = false;
        remainingCooldownTime = 0f;
        if (currentCommandAmount >= commandAmount)
        {
            gameOver();
            yield break;
        }
        nextCommand();
        yield break;
    }
    public void startGame()
    {
        StopAllCoroutines();
        gameStarted = true;
        currentCommandAmount = 0;
        judgementResults = new int[] {0, 0, 0, 0, 0};
        inputCounts = new int[] { 0, 0, 0, 0, 0, 0 };
        countDisplay.text = $"{currentCommandAmount} / {commandAmount}";
        judgementUI.SetActive(true);
        resultUI.SetActive(false);
        startHint.SetActive(false);
        StartCoroutine(CooldownCoroutine());
    }
    private void gameOver()
    {
        judgementUI.SetActive(false);
        resultUI.SetActive(true);
        string str = "";
        for (int i = 0; i < judgementResults.Length; i++)
        {
            str += $"{Enum.GetName(typeof(Judgements), i)}: {judgementResults[i]}\n";
        }
        resultDisplay.text = str;
        Invoke("showHint", 3f); 
    }
    private void showHint()
    {
        startHint.SetActive(true);
        gameStarted = false;
    }
    private void Start()
    {
        startGame();
    }
    private void Update()
    {
        timer += Time.deltaTime;
    }

}
