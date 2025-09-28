using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public enum JoystickZone{
    Origin,
    InnerCenter,
    OuterCenter,
    Up,
    Down,
    Left,
    Right
}

public class CatAnimationControl : MonoBehaviour
{
    [SerializeField] private AudioSource catSE;
    
    Animator cat_animator;
    public Animator cat_cube;
    //[Range(0f, 1f)] public float CubeAni = 0f;  // 動態控制進度

    public JudgementManager gameManager;
    public float originThreshold;//原點
    public float innerThreshold;//內圈=靜止
    public float outerThreshold;//外圈=左右手
    private JoystickZone currentZone = JoystickZone.Origin; // 當前搖桿區域

    public float freq;
    public float amp;

    //[SerializeField] private AudioSource audioSource;

    [Header("Audio Clips for Zones")]
    [SerializeField] private AudioClip originSE;
    [SerializeField] private AudioClip innercenterSE;
    [SerializeField] private AudioClip outercenterSE;
    [SerializeField] private AudioClip upSE;
    [SerializeField] private AudioClip downSE;
    [SerializeField] private AudioClip leftSE;
    [SerializeField] private AudioClip rightSE;

    private bool canPlaySE = true;

    [Header("音效")]
    [SerializeField]private AudioClip pressLaRSE;
    [SerializeField]private AudioClip pressUaDSE;
    //public AudioClip SqueezeX_Sound;
    //public AudioClip SqueezeY_Sound;
    //public AudioClip pullCatTail;
    //public AudioClip pullCatHand;
    private AudioSource catAudioSound;
    [Header("物件")]
    public GameObject CatFace;
    private Vector3 scaleZ, scaleX;
    private float press_trigger, press_grip, pull_joystick_x, pull_joystick_y;
    private bool isholdingTrigger, isholdingGrip = false;
 
    //[Header("擠壓程度")]
    //public float squeezeX, squeezeZ = 1;
    // private float Joystick_1, Joystick_2, Joystick_3, Joystick_4;
    // public float up_stick_upper_limit, up_stick_lower_limit, down_stick_upper_limit, down_stick_lower_limit, left_stick_upper_limit, left_stick_lower_limit, right_stick_upper_limit, right_stick_lower_limit;


    [Header("UI")]
    // public TextMeshProUGUI axisStatusText;
    public TextMeshProUGUI AxisXStatusText;
    public TextMeshProUGUI AxisYStatusText;
    public TextMeshProUGUI triggerStatusText;
    public TextMeshProUGUI gripStatusText;
    //public Slider gripSlider, AxisXSlider, AxisYSlider;
    public JoystickZone GetJoystickZone(float x, float y){
        float absX = Mathf.Abs(x);
        float absY = Mathf.Abs(y);
        //判斷原點
        //if (absX < originThreshold && absY < originThreshold){
        //    return JoystickZone.Origin;
        //}
        ////判斷內圈
        //if (absX < innerThreshold && absY < innerThreshold){
        //    return JoystickZone.InnerCenter;
        //}
        ////判斷外圈
        //if (absX < outerThreshold && absY < outerThreshold){
        //    return JoystickZone.OuterCenter;
        //}
        //判斷上下左右    
        if (absX < absY){
            if (y > 0){
                return JoystickZone.Up;
            }
            else{
                return JoystickZone.Down;
            }
        }
        else if (absX > absY){
            if (x > 0){
                return JoystickZone.Right;
            }else{
                return JoystickZone.Left;
            }
        }else{
            return JoystickZone.Origin;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        catAudioSound = GetComponent<AudioSource>();
        cat_animator = GetComponent<Animator>();
        cat_cube = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.SetControllerVibration(1,10,OVRInput.Controller.RTouch);
        //OVRInput.SetControllerVibration(1, 10, OVRInput.Controller.LTouch);




        pull_joystick_x = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
        pull_joystick_y = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
        press_trigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
        press_grip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
        float x = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
        float y = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
        //JoystickZone currentZone = GetJoystickZone(x, y);
        //Debug.Log($"Current Zone: {currentZone}");
        JoystickZone newZone = GetJoystickZone(x, y);
        if (newZone != currentZone){
            currentZone = newZone;
            //Debug.Log($"當前區域變更為: {currentZone}");
            PerformActionBasedOnZone(newZone);
            //PerformActionBasedOnZone(currentZone); // 根據區域執行行為
            cat_cube.SetFloat("PullEar", 0);
            cat_cube.SetFloat("PullTail", 0);
            cat_cube.SetFloat("PullLeftHand", 0);
            cat_cube.SetFloat("PullRightHand", 0);
        }else{
            PerformActionBasedOnZone(currentZone);
        }
        // if(){
        //     cat_cube.SetFloat("UPPull",y);
        //     Debug.Log("cat_cube"+cat_cube);
        // }

        // if(currentZone=Up){
        //     cat_cube.SetFloat("UPPull",press_trigger);
        // }

        ///////Back and Front Squeeze Detect
        //press_trigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
        // scaleZ = new Vector3(1.0f, 1.0f, 1.0f);
        if (press_trigger > 0 && !isholdingTrigger)
        {
            //scaleZ = new Vector3(1.0f, 1.0f, 1 - press_trigger*squeezeZ);

            //CatFace.transform.localScale = scaleZ;
            //PlayPressUaDSE();
            //Debug.Log("播放: PlayPressUaDSE");
            //cat_cube.SetFloat("UDPress", press_trigger);
            //JudgementManager.Instance.AddPress("Trigger");
            //GameManager.Instance.AddPress("Trigger");
            //if (press_trigger == 1)
            //    GameManager.Instance.MoveToNextStage(); // 通知 GameManager

            //OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.RTouch);
            //OVRInput.SetControllerVibration(freq, amp, OVRInput.Controller.LTouch);
            StartCoroutine(TriggerHandler());
        }
        // else
        // {
        //     cat_cube.SetFloat("UDPress", 0);
        // }
        
        triggerStatusText.text = press_trigger.ToString();
        
        //if(OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger)){
        //    scaleZ = new Vector3(0.0f, 0.0f, 0.0f);
        //    Debug.Log("RecoverZ");
        //}

        ///////Left and Right Squeeze Detect
        //press_grip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
       // scaleX = new Vector3(1.0f, 1.0f, 1.0f);
        if (press_grip > 0 && !isholdingGrip){
            //scaleX = new Vector3(1 - press_grip*squeezeX, 1.0f, 1.0f);
            //Debug.Log("Grip :" + press_grip);
            //CatFace.transform.localScale = scaleX;
            //cat_cube.SetFloat("LRPress",press_grip);
            //PlayPressLaRSE();
            //Debug.Log("播放: PlayPressLaRSE");
            //JudgementManager.Instance.AddPress("Grip");
            StartCoroutine(GripHandler());
        }
        // else{
        //     cat_cube.SetFloat("LRPress",0);
        // }
        gripStatusText.text = press_grip.ToString();
        //gripSlider.value = press_grip;

        //if(OVRInput.GetUp(OVRInput.RawButton.RHandTrigger)){
        //    scaleX = new Vector3(1.0f, 1.0f, 1.0f);
        //    Debug.Log("RecoverX");
        //}

        ///////////RightHand Thumbstick Detect
         //pull_joystick_x=OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
         //pull_joystick_y=OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
        // if(pull_joystick_x > 0 && pull_joystick_y > 0){//(+,+)
        //     //Debug.Log("ThumbstickX: " + pull_joystick_x+" , ThumbstickY: "+pull_joystick_y);
        //     Debug.Log("11111");
        //     cat_cube.SetFloat("Joystick_1", 1);
        // }else{
        //     cat_cube.SetFloat("Joystick_1", 0);
        // }
        // if(pull_joystick_x < 0 && pull_joystick_y > 0){//(-,+)
        //     //Debug.Log("ThumbstickX: " + pull_joystick_x+" , ThumbstickY: "+pull_joystick_y);
        //     Debug.Log("22222");
        //     cat_cube.SetFloat("Joystick_2", 1);
        // }else{
        //     cat_cube.SetFloat("Joystick_2", 0);
        // }
        // if(pull_joystick_x < 0 && pull_joystick_y < 0){//(-,-)
        //     //Debug.Log("ThumbstickX: " + pull_joystick_x+" , ThumbstickY: "+pull_joystick_y);
        //     Debug.Log("33333");
        //     cat_cube.SetFloat("Joystick_3", 1);
        // }else{
        //     cat_cube.SetFloat("Joystick_3", 0);
        // }
        // if(pull_joystick_x > 0 && pull_joystick_y < 0){//(+,-)
        //     //Debug.Log("ThumbstickX: " + pull_joystick_x+" , ThumbstickY: "+pull_joystick_y);
        //     Debug.Log("44444");
        //     cat_cube.SetFloat("Joystick_4", 1);
        // }else{
        //     cat_cube.SetFloat("Joystick_4", 0);
        // }



        // AxisXSlider.value=pull_joystick_x;
        // AxisYSlider.value=pull_joystick_y;        
        AxisXStatusText.text= pull_joystick_x.ToString();
        AxisYStatusText.text= pull_joystick_y.ToString();
        



        // if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y < 0) {
        //     catAudioSound.PlayOneShot(pullCatTail);
        //     Debug.Log("Thumbstick Down :"+OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y); 
        // }

        //cat_cube.SetFloat("cat_cube", CubeAni);

        // //trigger
        // if (OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger) > 0) {
        //     cat_animator.SetFloat("SqueezeY",OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger));
        //     catAudioSound .PlayOneShot(SqueezeY_Sound);
        //     Debug.Log("squeezeY"+OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger));
        // }
        // if (OVRInput.GetUp(OVRInput.RawButton.RIndexTrigger)) {
        //     Debug.Log("Now: "+OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger));
        // }

        // //Grip
        // if (OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger) > 0) {
        //     //  Debug.Log("R Grip :"+OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger));
        //     cat_animator.SetFloat("SqueezeX",OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger));
        //     catAudioSound .PlayOneShot(SqueezeX_Sound);
        //     Debug.Log("squeezeX"+OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger));
        // }



    }
    private IEnumerator GripHandler()
    {
        isholdingGrip = true;
        PlayPressLaRSE();
        //Debug.Log("播放: PlayPressLaRSE");
        JudgementManager.Instance.AddPress("Grip");
        while (press_grip > 0)
        {
            Debug.Log("Grip :" + press_grip);
            cat_cube.SetFloat("LRPress", press_grip);
            yield return null;
        }
        cat_cube.SetFloat("LRPress", 0.0f);
        isholdingGrip = false;
        yield break;
    }
    private IEnumerator TriggerHandler()
    {
        isholdingTrigger = true;
        PlayPressUaDSE();
        //Debug.Log("播放: PlayPressUaDSE");
        JudgementManager.Instance.AddPress("Trigger");
        while (press_trigger > 0)
        {
            Debug.Log("Trigger :" + press_trigger);
            cat_cube.SetFloat("UDPress", press_trigger);
            yield return null;
        }
        cat_cube.SetFloat("UDPress", 0.0f);
        isholdingTrigger = false;
        yield break;
    }



    void PerformActionBasedOnZone(JoystickZone zone)
    {
        switch (zone)
        {
            //case JoystickZone.Origin:
            //    Debug.Log("搖桿在原點：執行idle");
            //    OriginPoint();
            //    break;
            //case JoystickZone.InnerCenter:
            //    Debug.Log("搖桿在內圈：輕微抖動");
            //    OriginSlow();
            //    break;
            //case JoystickZone.OuterCenter:
            //    Debug.Log("搖桿在外圈：拉雙手");
            //    PullDoubleHand();
            //    break;
            case JoystickZone.Up:
                // Debug.Log("搖桿在上區域：拉耳朵");
                PullEar();
                JudgementManager.Instance.AddPress("JoystickUp");
                break;
            case JoystickZone.Down:
                // Debug.Log("搖桿在下區域：拉尾巴");
                PullTail();
                JudgementManager.Instance.AddPress("JoystickDown");
                break;
            case JoystickZone.Left:
                // Debug.Log("搖桿在左區域：拉左手");
                PullLeftHand();
                JudgementManager.Instance.AddPress("JoystickLeft");
                break;
            case JoystickZone.Right:
                // Debug.Log("搖桿在右區域：拉右手");
                PullRightHand();
                JudgementManager.Instance.AddPress("JoystickRight");
                break;
            default:
                // Debug.Log("未知區域");
                break;
        }
    }

    //void OriginPoint()
    //{
    //    PlayPressSound(originSE);
    //}
    //void OriginSlow()
    //{
    //    PlayPressSound(innercenterSE);
    //}
    //void PullDoubleHand()
    //{
    //    PlayPressSound(outercenterSE);
    //}
    void PullEar()
    {
        //PlaySE();
        cat_cube.SetFloat("PullEar", pull_joystick_y);
        PlayPressSound(upSE);
        Debug.Log("播放: upSE");
    }
    void PullTail()
    {
        cat_cube.SetFloat("PullTail", pull_joystick_y);
        PlayPressSound(downSE);
        Debug.Log("播放: downSE");
    }
    void PullLeftHand()
    {
        cat_cube.SetFloat("PullLeftHand", pull_joystick_x);
        PlayPressSound(leftSE);
        Debug.Log("播放: leftSE");
    }
    void PullRightHand()
    {
        cat_cube.SetFloat("PullRightHand", pull_joystick_x);
        PlayPressSound(rightSE);
        Debug.Log("播放: rightSE");
    }

    private void PlayPressUaDSE(){
        if(catAudioSound != null && canPlaySE)
        {
            //catAudioSound.Play();
            PlayPressSound(pressUaDSE);
            canPlaySE = false;
            StartCoroutine(ResetPlayState());
        }
    }

    private void PlayPressLaRSE() {
        //if (catAudioSound != null && canPlaySE)
        if (canPlaySE)
        {
            PlayPressSound(pressLaRSE);
            //catAudioSound.Play(pressLaRSE);
            canPlaySE = false;
            StartCoroutine(ResetPlayState());
        }
    }

    private void PlayPressSound(AudioClip clip) {
        if (clip == null)
        {
            Debug.LogWarning("AudioClip is null!");
            return;
        }

        catSE.clip = clip;
        catSE.Play();

        StartCoroutine(ResetPlayState());
    }
    private IEnumerator ResetPlayState(){
        if (catAudioSound == null)
        {
            Debug.LogError("在協程中發現 catAudioSound 為 null！");
            yield break; // 提前退出協程
        }
       
        yield return new WaitForSeconds(catAudioSound.clip.length);
        canPlaySE = true;

        if (catAudioSound != null)
        {
            Debug.Log("冷卻完成，音效可以再次播放！");
        }
    }

    
}
