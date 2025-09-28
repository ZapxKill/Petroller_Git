using System.Collections;
using System.Collections.Generic;
// using Oculus.Interaction.Samples;
using UnityEngine;
using static OVRVirtualKeyboard;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CatAnimationControl playpressSE;
    [Header("���q����")]
    public GameObject[] stages; // �s��C�Ӷ��q������

    private int currentStage = 0; // ���e���q���� (�q0�}�l)
    private int pressCount = 0;    // �������ƭp�ƾ�
    public int requiredPresses; // �C���q�ݭn����������
    private bool isCooldown = false; // �O�_�B��N�o��
    public float cooldownTime; // ���s�����᪺�N�o�ɶ��]���^
    private float remainingCooldownTime; // �Ѿl�N�o�ɶ�

    // �C�ӿ�J�ӷ����p�ƾ�
    private int joystickUpCount = 0;
    private int joystickDownCount = 0;
    private int joystickLeftCount = 0;
    private int joystickRightCount = 0;
    private int triggerPressCount = 0;//UaDPress count
    private int gripPressCount = 0;//LaRPress count

    private float timer = 0f; // �Ω�p�ɪ��ܼ�
    public float countdownTime = 5f; // �]�m�˼ƭp�ɬ���
    public int final_stage_countdown;

    [SerializeField] private AudioClip transSceneSE;
    
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


    // �W�[�����p��
    public virtual void AddPress(string inputSource) {
        if (isCooldown) return; // �p�G�B��N�o�ɶ��A������J

        // �ھڿ�J�ӷ��W�[�p��
        switch (inputSource)
        {
            case "JoystickUp":
                joystickUpCount++;
                break;
            case "JoystickDown":
                joystickDownCount++;
                break;
            case "JoystickLeft":
                joystickLeftCount++;
                break;
            case "JoystickRight":
                joystickRightCount++;
                break;
            case "Trigger":
                triggerPressCount++;
                break;
            case "Grip":
                gripPressCount++;
                break;
            default:
                break;
        }

        pressCount++; // �W�[�`�p�Ʀ���

        // �ˬd�O�_�F��n�D����������
        if ((currentStage == 0) && (gripPressCount >= requiredPresses))
        {
            SwitchToNextStage();
            ResetCounter();
           // Debug.Log("currentStage: "+ currentStage+ " , gripPressCount: "+ gripPressCount + ", requiredPresses, " + requiredPresses);
        }
        if ((currentStage == 1) && (triggerPressCount >= requiredPresses))
        {
            SwitchToNextStage();
            ResetCounter();
            //Debug.Log("currentStage: " + currentStage + " , gripPressCount: " + gripPressCount + ", requiredPresses, " + requiredPresses);
        }
        if (currentStage == 2 && gripPressCount >= requiredPresses)
        {
            SwitchToNextStage();
            ResetCounter();
            //Debug.Log("currentStage: " + currentStage + " , gripPressCount: " + gripPressCount + ", requiredPresses, " + requiredPresses);
        }
        if (currentStage == 3 && joystickUpCount >= requiredPresses)
        {
            SwitchToNextStage();
            ResetCounter();
            //Debug.Log("Time Sec: " + timer);
        }
        if (currentStage == 4 && joystickRightCount >= requiredPresses)
        {
            SwitchToNextStage();
            ResetCounter();
            //Debug.Log("Time Sec: " + timer);
        }
        if (currentStage == 5 && joystickLeftCount >= requiredPresses)
        {
            SwitchToNextStage();
            ResetCounter();
            timer = 0f;// ���m�p��
            //Debug.Log("Time Sec: "+ timer);
        }
        if (currentStage == 6 && timer >= final_stage_countdown)
        {
            SwitchToNextStage();
            ResetCounter();
        }
  

        // �ҰʧN�o�ɶ�
        StartCoroutine(CooldownCoroutine());
    }

    private void ResetCounter() {
        joystickUpCount = 0;
        joystickDownCount = 0;
        joystickLeftCount = 0;
        joystickRightCount = 0;
        triggerPressCount = 0;
        gripPressCount = 0;
        pressCount = 0;
    }

    //// �ΨӼW�[�������ƨåB��s�޿�
    //public void AddPress() {
    //    // �ˬd�O�_�b�N�o��
    //    if (isCooldown) return;

    //    pressCount++; // �W�[��������
    //    Debug.Log($"���e�������ơG{pressCount}/{requiredPresses}");

    //    if (pressCount >= requiredPresses) // �p�G�F��n�D���������ơA�������q
    //    {
    //        SwitchToNextStage();
    //    }

    //    // �}�ҧN�o�ɶ�
    //    StartCoroutine(CooldownCoroutine());
    //}

    // ������U�@���q
    public void SwitchToNextStage() {
        stages[currentStage].SetActive(false);
        currentStage++;
        stages[currentStage].SetActive(true);
        ////pressCount = 0; // �k�s�p�ƾ�
        if (currentStage >= stages.Length)
        {
            Debug.Log("�w�F��̤j���q�A�L�k�A�����I");
            return;
        }
        //// ���÷��e���q������
        //if (currentStage < stages.Length && stages[currentStage] != null)
        //{
        //    stages[currentStage].SetActive(false);  // ���÷��e���q����
        //}
        //currentStage++;
        //// ��ܤU�@���q������
        //if (currentStage < stages.Length && stages[currentStage] != null)
        //{
        //    stages[currentStage].SetActive(true);
        //}

        // �i�J�U�@���q�A�p�ƾ��k�s

        Debug.Log($"�����춥�q {currentStage}");
    }

    // �N�o��{�A����N�o�ɶ�
    private IEnumerator CooldownCoroutine() {
        isCooldown = true; // �]�w���N�o��
        remainingCooldownTime = cooldownTime; // ��l�ƳѾl�N�o�ɶ�

        while (remainingCooldownTime > 0) // ���N�o�ɶ��j�� 0 ��
        {
            remainingCooldownTime -= Time.deltaTime; // ��ֳѾl�N�o�ɶ�
            yield return null; // ���ݤU�@�V
        }

        isCooldown = false; // �N�o�ɶ�����
        remainingCooldownTime = 0; // �N�o�ɶ��k�s
    }
       
        // ��s�C�V���Ѿl�ɶ��^��
        private void Update() {
        if (isCooldown)
        {
            //Debug.Log($"�Ѿl�N�o�ɶ��G{remainingCooldownTime:F0}��"); // ��X�Ѿl�N�o�ɶ�
        }
        //Debug.Log("�ثe���q" + currentStage + "," + "�ثe����" + pressCount);


        timer += Time.deltaTime; // �C�V�֥[�ɶ�
        //Debug.Log(timer);
        //if (timer == 30)
        //{
        //    Debug.Log(timer);
        //}
        //if (timer >= countdownTime) // �P�_�p�ɬO�_����
        //{
        //    Debug.Log("�ɶ���I");
        //    timer = 0f; // ���m�p��
        //}

    }

    //// ����C�Ӥ�V����������
    //public int GetJoystickUpCount() {
    //    return joystickUpCount;
    //}

    //public int GetJoystickDownCount() {
    //    return joystickDownCount;
    //}

    //public int GetJoystickLeftCount() {
    //    return joystickLeftCount;
    //}

    //public int GetJoystickRightCount() {
    //    return joystickRightCount;
    //}
    //public int GettriggerPressCount() {
    //    return triggerPressCount;
    //}
    //public int GetGripPressCount() {
    //    return gripPressCount;
    //}
}
