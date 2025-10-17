using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Oculus.Interaction.Samples;
using TMPro;
using Unity.VisualScripting;

// using Oculus.Interaction.Samples;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using static OVRVirtualKeyboard;
using System.Linq;
public class RecordData
{
    public string timeStamp;
    public float reactionTime;
    public string command;
    public bool corrected;
    public RecordData(float reactionTime, string command, bool corrected)
    {
        timeStamp = DateTime.Now.ToString("HH:mm:ss");
        this.reactionTime = reactionTime;
        this.command = command;
        this.corrected = corrected;
    }
}
public class JudgementManager : MonoBehaviour
{
    public static JudgementManager Instance;
    private bool isCooldown = false;
    private bool gameStarted = false;
    private float remainingCooldownTime;
    [Header("Record setting")]
    public bool recordPlayResult = false;
    public string recordPath = Application.dataPath + "/records";
    private List<RecordData> recordDatas = new List<RecordData>();
    [Header("Game setting")]
    [SerializeField] private float minCooldownTime;
    [SerializeField] private float maxCooldownTime;
    private int[] inputCounts = { 0, 0, 0, 0, 0, 0 };

    private int[] judgementResults = { 0, 0, 0, 0, 0 }; // Perfect Great Good Bad Miss
    [SerializeField] private int commandAmount;
    private int currentCommandAmount = 0;
    private float totalRecationtime = 0f;
    //private enum Judgements { Perfect, Great, Good, Bad, Miss }
    [Header("Judgement setting")]
    [SerializeField] private float[] judgementTiming = { 5f, 3f, 1f, 0f }; // Perfect Great Good Bad
    private float remainingjudgementTime = 0f;
    [SerializeField] float judgementTime = 7f;

    [Header("Sound Setting")]
    [SerializeField] AudioSource catAudioSource;
    [SerializeField] AudioClip[] catAudios;
    [Header("UI Setting")]
    [SerializeField] GameObject judgementUI;
    [SerializeField] GameObject resultUI;
    [SerializeField] GameObject startHint;
    [SerializeField] GameObject bubbleUI;
    [SerializeField] Image percentageCircle;
    [SerializeField] TextMeshProUGUI resultDisplay;
    [SerializeField] TextMeshProUGUI countDisplay;
    [SerializeField] GameObject[] particles;
    public enum InputCommands { JoystickUp, JoystickDown, JoystickLeft, JoystickRight, Trigger, Grip }
    private string[] commands = { "耳朵", "尾巴", "左手", "右手", "壓頭", "擠壓" };
    private string currentCommand = "";
    private int commandIndex = 0;
    private float completePercent = 0;
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
                    particles[i].SetActive(true);
                    judgementResults[i]++;
                    catAudioSource.clip = catAudios[i];
                    catAudioSource.Play();
                    if (recordPlayResult)
                    {
                        recordDatas.Add(new RecordData(judgementTime - remainingjudgementTime, commands[commandIndex], true));
                    }
                    remainingjudgementTime = -1f;
                    return;
                }
            }
        }
        particles[4].SetActive(true);
        judgementResults[4]++;
        catAudioSource.clip = catAudios[4];
        catAudioSource.Play();
        if (recordPlayResult)
        {
            recordDatas.Add(new RecordData(judgementTime - remainingjudgementTime, commands[commandIndex], false));
        }
        
        remainingjudgementTime = -1f;
        return;
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
        bubbleUI.SetActive(true);
        StartCoroutine(JudgementCoroutine());
    }

    private IEnumerator JudgementCoroutine()
    {
        remainingjudgementTime = judgementTime;
        while (remainingjudgementTime > 0)
        {
            remainingjudgementTime -= Time.deltaTime;
            yield return null;
        }
        //Debug.Log("Time Up");

        if (remainingjudgementTime != -1f)
        {
            particles[4].SetActive(true);
            judgementResults[4]++;
            totalRecationtime += judgementTime;
            catAudioSource.clip = catAudios[4];
            catAudioSource.Play();
            if (recordPlayResult)
            {
                recordDatas.Add(new RecordData(judgementTime, commands[commandIndex], false));
            }
            
        }
        remainingjudgementTime = 0f;
        currentCommandAmount = 0;
        for (int i = 0; i < judgementResults.Length; i++)
        {
            currentCommandAmount += judgementResults[i];
        }
        countDisplay.text = $"{currentCommandAmount} / {commandAmount}";
        bubbleUI.SetActive(false);
        StartCoroutine(CooldownCoroutine());
        yield break;
    }
    private IEnumerator CooldownCoroutine()
    {
        isCooldown = true;
        remainingCooldownTime = UnityEngine.Random.Range(minCooldownTime, maxCooldownTime);
        commandAnimator.SetInteger("index", 6);
        //Debug.Log("Cool Down");
        while (remainingCooldownTime > 0f)
        {
            remainingCooldownTime -= Time.deltaTime;
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
        if (recordPlayResult)
        {
            recordDatas.Clear();
        }
        gameStarted = true;
        currentCommandAmount = 0;
        judgementResults = new int[] {0, 0, 0, 0, 0};
        inputCounts = new int[] { 0, 0, 0, 0, 0, 0 };
        countDisplay.text = $"{currentCommandAmount} / {commandAmount}";
        judgementUI.SetActive(true);
        bubbleUI.SetActive(false);
        resultUI.SetActive(false);
        startHint.SetActive(false);
        StartCoroutine(CooldownCoroutine());
    }
    private void gameOver()
    {
        judgementUI.SetActive(false);
        resultUI.SetActive(true);
        string str = "";
        completePercent = 0f;
        for (int i = 0; i < judgementResults.Length; i++)
        {
            completePercent += judgementResults[i] * (judgementResults.Length - 1 - i);
        }
        completePercent /= commandAmount * (judgementResults.Length - 1);
        str += $"{(completePercent*100f).ToString("F0")}%\n";
        percentageCircle.fillAmount = completePercent;
        //str += $"AvgTime: {totalRecationtime / commandAmount}";
        resultDisplay.text = str;
        Invoke("showHint", 3f);
        if (recordPlayResult)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(recordPath, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".csv"), false, System.Text.Encoding.UTF8))
            {
                float avgTime = 0f;
                float correctRate = 0;
                writer.WriteLine("時間,反應時間,動作,正確");
                foreach (RecordData data in recordDatas)
                {
                    writer.WriteLine($"{data.timeStamp},{data.reactionTime}s,{data.command},{data.corrected}");
                    avgTime += data.reactionTime;
                    if (data.corrected)
                    {
                        correctRate += 1f;
                    }
                }
                writer.WriteLine($"平均時間,{avgTime/commandAmount}s,正確率,{correctRate/commandAmount*100}%");
                writer.Close();
            }
        }
    }
    private void showHint()
    {
        startHint.SetActive(true);
        gameStarted = false;
    }
    private void Start()
    {
        if (!Directory.Exists(recordPath) && recordPlayResult)
        {
            Directory.CreateDirectory(recordPath);
        }
        //startGame();
    }
    private void Update()
    {
        timer += Time.deltaTime;
    }

}
