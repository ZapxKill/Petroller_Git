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

    Animator cat_animator;
    public Animator cat_cube;
    [SerializeField] private ControllerFollower controllerFollower;
    private OVRInput.Controller controller;
    //[Range(0f, 1f)] public float CubeAni = 0f;  // 動態控制進度
    private JoystickZone currentZone = JoystickZone.Origin; // 當前搖桿區域
    private Vector2 originOffset = Vector2.zero;
    [SerializeField] private float deadZoneValue = 0.2f;
    private bool offsetAdjustButton = false;
    private float offsetAdjustTimer = 0f;
    [Header("物件")]
    public GameObject CatFace;
    private Vector3 scaleZ, scaleX;
    private float press_trigger, press_grip, pull_joystick_x, pull_joystick_y;
    private bool isholdingTrigger, isholdingGrip = false;


    [Header("UI")]
    [SerializeField] Image adjustTimeCircle;
    public TextMeshProUGUI AxisXStatusText;
    public TextMeshProUGUI AxisYStatusText;
    public TextMeshProUGUI triggerStatusText;
    public TextMeshProUGUI gripStatusText;
    //public Slider gripSlider, AxisXSlider, AxisYSlider;
    public JoystickZone GetJoystickZone(float x, float y) {
        float absX = Mathf.Abs(x - originOffset.x);
        float absY = Mathf.Abs(y - originOffset.y);
        // Debug.Log($"absX: {absX}");
        // Debug.Log($"absY: {absY}");
        // Debug.Log($"當前區域變更為: {currentZone}");
        if (absX > deadZoneValue || absY > deadZoneValue)
        {
            if (absX < absY)
            {
                if (y - originOffset.y > 0)
                {
                    return JoystickZone.Up;
                }
                return JoystickZone.Down;
            }
            if (absX > absY)
            {
                if (x - originOffset.x > 0)
                {
                    return JoystickZone.Right;
                }
                return JoystickZone.Left;
            }

        }
        return JoystickZone.Origin;

    }
    // Start is called before the first frame update
    void Start()
    {
        controller = controllerFollower.targetController;
        cat_animator = GetComponent<Animator>();
        cat_cube = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //OVRInput.SetControllerVibration(1,10,OVRInput.Controller.RTouch);
        //OVRInput.SetControllerVibration(1, 10, OVRInput.Controller.LTouch);
        if (controller == OVRInput.Controller.RTouch)
        {
            pull_joystick_x = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;
            pull_joystick_y = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).y;
            press_trigger = OVRInput.Get(OVRInput.RawAxis1D.RIndexTrigger);
            press_grip = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);
            offsetAdjustButton = OVRInput.Get(OVRInput.RawButton.A);
        }
        else if (controller == OVRInput.Controller.LTouch)
        {
            pull_joystick_x = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).x;
            pull_joystick_y = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
            press_trigger = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            press_grip = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
            offsetAdjustButton = OVRInput.Get(OVRInput.RawButton.X);
        }
        else
        {
            Debug.LogWarning("控制器選擇錯誤");
        }
        if (offsetAdjustButton && offsetAdjustTimer == 0f)
        {
            StartCoroutine(offsetAdjust());
        }
    JoystickZone newZone = GetJoystickZone(pull_joystick_x, pull_joystick_y);
        if (newZone != currentZone){
            currentZone = newZone;   
            PerformActionBasedOnZone(newZone);
    cat_cube.SetFloat("PullEar", 0);
            cat_cube.SetFloat("PullTail", 0);
            cat_cube.SetFloat("PullLeftHand", 0);
            cat_cube.SetFloat("PullRightHand", 0);
        }
        else{
            PerformActionBasedOnZone(currentZone);
        }
        if (press_trigger > 0 && !isholdingTrigger)
        {
            StartCoroutine(TriggerHandler());
        }
        
        triggerStatusText.text = press_trigger.ToString();
        
        if (press_grip > 0 && !isholdingGrip){
            StartCoroutine(GripHandler());
        }
        gripStatusText.text = press_grip.ToString();     
        AxisXStatusText.text= pull_joystick_x.ToString();
        AxisYStatusText.text= pull_joystick_y.ToString();
    }
    private IEnumerator GripHandler()
    {
        isholdingGrip = true;
        //PlayPressLaRSE();
        //Debug.Log("播放: PlayPressLaRSE");
        JudgementManager.Instance.AddPress("Grip");
        while (press_grip > 0)
        {
            //Debug.Log("Grip :" + press_grip);
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
        //PlayPressUaDSE();
        //Debug.Log("播放: PlayPressUaDSE");
        JudgementManager.Instance.AddPress("Trigger");
        while (press_trigger > 0)
        {
            //Debug.Log("Trigger :" + press_trigger);
            cat_cube.SetFloat("UDPress", press_trigger);
            yield return null;
        }
        cat_cube.SetFloat("UDPress", 0.0f);
        isholdingTrigger = false;
        yield break;
    }
    private IEnumerator offsetAdjust()
    {
        Debug.Log("offsetAdjusting");
        adjustTimeCircle.gameObject.SetActive(true);
        while (offsetAdjustButton && offsetAdjustTimer <= 3.0f)
        {
            offsetAdjustTimer += Time.deltaTime;
            adjustTimeCircle.fillAmount = offsetAdjustTimer / 3.0f;
            yield return null;
        }
        adjustTimeCircle.gameObject.SetActive(false);
        adjustTimeCircle.fillAmount = 0f;
        originOffset = new Vector2(pull_joystick_x, pull_joystick_y);
        while (offsetAdjustButton)
        {
            yield return null;
        }
        offsetAdjustTimer = 0f;
        yield break;
    }



    void PerformActionBasedOnZone(JoystickZone zone)
    {
        switch (zone)
        {
            case JoystickZone.Up:
                // Debug.Log("搖桿在上區域：拉耳朵");
                cat_cube.SetFloat("PullEar", pull_joystick_y);
                JudgementManager.Instance.AddPress("JoystickUp");
                break;
            case JoystickZone.Down:
                // Debug.Log("搖桿在下區域：拉尾巴");
                cat_cube.SetFloat("PullTail", pull_joystick_y);
                JudgementManager.Instance.AddPress("JoystickDown");
                break;
            case JoystickZone.Left:
                // Debug.Log("搖桿在左區域：拉左手");
                cat_cube.SetFloat("PullLeftHand", pull_joystick_x);
                JudgementManager.Instance.AddPress("JoystickLeft");
                break;
            case JoystickZone.Right:
                // Debug.Log("搖桿在右區域：拉右手");
                cat_cube.SetFloat("PullRightHand", pull_joystick_x);
                JudgementManager.Instance.AddPress("JoystickRight");
                break;
            default:
                // Debug.Log("未知區域");
                break;
        }
    }
}
