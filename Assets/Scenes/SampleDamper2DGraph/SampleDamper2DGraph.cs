//--------------------------------------------------------------------
// Created by Alexis Bacot - 2021 - www.alexisbacot.com
//--------------------------------------------------------------------
using UnityEngine;

//--------------------------------------------------------------------
public class SampleDamper2DGraph : MonoBehaviour
{
    //--------------------------------------------------------------------
    public enum EnumDampingType
    {
        None,
        Normal,
        Spring,
        UnitySmoothDamp,
        SpringCritical,
        SpringCriticalNoGoalSpeed,
        DoubleCriticalNoGoalSpeed,
        //TimedCriticalNoGoalSpeed,
    }

    //--------------------------------------------------------------------
    [Header("Damping Params")]
    public EnumDampingType DampingType = EnumDampingType.Spring;
    public float halflife = 0.5f;
    public float frequency = 0.5f;
    public float speedMoveRight = 20.0f;

    //[Header("Timed Damping Params")]
    //public float timeToGoal = 1.0f;
    //public float apprehension = 2.0f;

    [Header("Goals Params")]
    public float goalMinY;
    public float goalMaxY, goalMoveSpeed;

    [Header("Links")]
    public Transform tsfmGoal;
    public Transform tsfmFollowing;
    public Camera cam;

    // Internal
    private Vector3 _posFollower, _posGoal, _posCam;
    private float _speedFollower;
    private float _speedGoal;
    // Another set of position and speed used for the double damper
    private float posGoal2, speedGoal2;

    //--------------------------------------------------------------------
    private void Update()
    {
        _posGoal = tsfmGoal.position;
        _posFollower = tsfmFollowing.position;
        _posCam = cam.transform.position;

        MoveGoal();

        // While changing values in the editor sometimes these values will be 0, we want to skip these!
        if (halflife == 0 || frequency == 0) return;

        // DAMPING: Follower y pos tries to match goal y
        switch(DampingType)
        {
            case EnumDampingType.Normal: _posFollower.y = ToolDamper.damper(_posFollower.y, _posGoal.y, halflife, Time.deltaTime); break;
            case EnumDampingType.Spring: ToolDamper.damper_spring(ref _posFollower.y, ref _speedFollower, _posGoal.y, _speedGoal, frequency, halflife, Time.deltaTime); break;
            case EnumDampingType.UnitySmoothDamp: _posFollower.y = Mathf.SmoothDamp(_posFollower.y, _posGoal.y, ref _speedFollower, halflife); break;
            case EnumDampingType.SpringCritical: ToolDamper.damper_spring_critical(ref _posFollower.y, ref _speedFollower, _posGoal.y, _speedGoal, halflife, Time.deltaTime); break;
            case EnumDampingType.SpringCriticalNoGoalSpeed: ToolDamper.damper_spring_critical_noGoalSpeed(ref _posFollower.y, ref _speedFollower, _posGoal.y, halflife, Time.deltaTime); break;
            case EnumDampingType.DoubleCriticalNoGoalSpeed: ToolDamper.double_spring_damper_implicit(ref _posFollower.y, ref _speedFollower, ref posGoal2, ref speedGoal2, _posGoal.y, halflife, Time.deltaTime); break;
            //case EnumDampingType.TimedCriticalNoGoalSpeed: ToolDamper.timed_spring_damper_implicit(ref _posFollower.y, ref _speedFollower, ref posGoal2, _posGoal.y, timeToGoal, halflife, Time.deltaTime, apprehension); break;
            default: Debug.LogWarning("[SampleDamper] DampingType: " + DampingType + " is not supported"); break;
        }

        // Move all to the right to have some trails behind follower
        _posFollower.x += speedMoveRight * Time.deltaTime;
        _posGoal.x += speedMoveRight * Time.deltaTime;
        _posCam.x += speedMoveRight * Time.deltaTime;

        // Finalize
        tsfmFollowing.position = _posFollower;
        tsfmGoal.position = _posGoal;
        cam.transform.position = _posCam;
    }

    //--------------------------------------------------------------------
    private void MoveGoal()
    {
        float newGoalY = 0;
        bool hasGoalChanged = false;

        // Use arrows to slowly change goal y position
        float inputVertical = Input.GetAxis("Vertical");
        if (inputVertical != 0)
        {
            newGoalY = _posGoal.y + inputVertical * goalMoveSpeed * Time.deltaTime;
            hasGoalChanged = true;
        }

        // Click to change goal y position instantly
        if (Input.GetMouseButtonDown(0))
        {
            newGoalY = cam.ScreenToWorldPoint(Input.mousePosition).y;
            hasGoalChanged = true;
        }

        if (hasGoalChanged)
        {
            newGoalY = Mathf.Clamp(newGoalY, goalMinY, goalMaxY);

            _speedGoal = (newGoalY - _posGoal.y) / Time.deltaTime;

            _posGoal.y = newGoalY;
        }
        else
            _speedGoal = 0.0f;
    }

    //--------------------------------------------------------------------
}

//--------------------------------------------------------------------