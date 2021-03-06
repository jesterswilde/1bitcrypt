using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.PlayerLoop;

[RequireComponent(typeof(Info))]
public class Character : MonoBehaviour, IComm
{
    [SerializeField]
    float speed = 16;
    Vector3 position;
    [SerializeField]
    Projectile projectilePrefab;
    [SerializeField]
    float jumpSpeed = 10;
    [SerializeField]
    float jumpDuration = 3;
    Vector3 lastDir = Vector3.up;
    ControlStyle control = ControlStyle.INPUT;
    MotionCurve motionCurve;
    Sprite baseSprite;
    public Sprite BaseSprite => baseSprite;
    SpriteRenderer rend;
    public Info Info { get; private set; }
    Animator anim;

    public int[] SenderID { get; } = new int[] { 1, 2, 3, 4, 5 };

    private void Awake()
    {
        position = transform.position;
        Info = GetComponent<Info>();
        rend = GetComponent<SpriteRenderer>();
        baseSprite = rend.sprite;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (control == ControlStyle.INPUT)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Shoot();
            }
            if (Input.GetKeyUp(KeyCode.LeftShift))
            {
                Jump();
            }
        }
    }

    private void FixedUpdate()
    {
        if (control == ControlStyle.INPUT)
        {
            Move();
        }
        else if (control == ControlStyle.CURVE)
        {
            MoveByCurve();
        }
        transform.position = Util.Round(position);
    }
    void MoveByCurve()
    {
        if (motionCurve == null)
        {
            control = ControlStyle.INPUT;
            return;
        }
        motionCurve.PassTime(Time.fixedDeltaTime);
        if (motionCurve.IsDone)
        {
            motionCurve = null;
            control = ControlStyle.INPUT;
        }
        else
            position += motionCurve.Movement * Time.fixedDeltaTime;
    }
    void Move()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        var dir = (Vector3.up * y + Vector3.right * x).normalized;
        anim.SetInteger("Horizontal", Mathf.RoundToInt(x));
        anim.SetInteger("Vertical", Mathf.RoundToInt(y));
        if (x != 0 || y != 0)
        {
            lastDir = dir;
        }
        position += dir * speed * Time.fixedDeltaTime;
    }
    void Jump()
    {
        control = ControlStyle.CURVE;
        motionCurve = Motion.GetCurve(jumpDuration, lastDir, jumpSpeed);
    }
    void Shoot()
    {
        var arrow = Instantiate(projectilePrefab);
        var arrowPos = Util.GetExtentPos(Info, Util.PosFromVec(lastDir));
        arrow.Setup(arrowPos, lastDir);
    }

    public void ReceiveMessage(Signal signal)
    {
        UIManager.DisplaySignal(signal, 5);
    }
}

enum ControlStyle
{
    INPUT,
    CURVE,
    NONE
}