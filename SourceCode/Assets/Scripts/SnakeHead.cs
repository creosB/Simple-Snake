using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : BodyPart
{
    Vector2 movement;

    private BodyPart tail = null;

    const float TIMETOADDBODYPART = 0.1f;
    float addTimer = TIMETOADDBODYPART;

    public int partsToAdd = 0;

    List<BodyPart> parts = new List<BodyPart>();

    public AudioSource[] gulpSounds = new AudioSource[3];
    public AudioSource dieSound = null;

    // Start is called before the first frame update
    void Start()
    {
        SwipeControls.OnSwipe += SwipeDetection;
    }

    // Update is called once per frame
    override public void Update()
    {
        if (!GameController.instance.alive) return;

        base.Update();

        SetMovement(movement * Time.deltaTime);
        UpdateDirection();
        UpdatePosition();

        if (partsToAdd > 0)
        {
            addTimer -= Time.deltaTime;
            if (addTimer<=0)
            {
                addTimer = TIMETOADDBODYPART;
                AddBodyPart();
                partsToAdd--;
            }
        }
    }

    void AddBodyPart()
    {
        if (tail == null)
        {
            Vector3 newPosition = transform.position;
            newPosition.z = newPosition.z + 0.01f;

            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab, newPosition, Quaternion.identity);
            newPart.following = this;
            tail = newPart;
            newPart.TurnIntoTail();

            parts.Add(newPart);
        }
        else
        {
            Vector3 newPosition = tail.transform.position;
            newPosition.z = newPosition.z + 0.01f;

            BodyPart newPart = Instantiate(GameController.instance.bodyPrefab, newPosition, tail.transform.rotation);
            newPart.following = tail;
            newPart.TurnIntoTail();
            tail.TurnIntoBodyPart();
            tail = newPart;

            parts.Add(newPart);
        }
    }

    void SwipeDetection(SwipeControls.SwipeDirection direction)
    {
        switch(direction)
        {
            case SwipeControls.SwipeDirection.Up:
                MoveUp();
                break;
            case SwipeControls.SwipeDirection.Down:
                MoveDown();
                break;
            case SwipeControls.SwipeDirection.Left:
                MoveLeft();
                break;
            case SwipeControls.SwipeDirection.Right:
                MoveRight();
                break;
        }
    }

    void MoveUp()
    {
        movement = Vector2.up * GameController.instance.snakeSpeed;
    }

    void MoveDown()
    {
        movement = Vector2.down * GameController.instance.snakeSpeed;
    }

    void MoveLeft()
    {
        movement = Vector2.left * GameController.instance.snakeSpeed;
    }

    void MoveRight()
    {
        movement = Vector2.right * GameController.instance.snakeSpeed;
    }

    public void ResetSnake()
    {
        foreach(BodyPart part in parts)
        {
            Destroy(part.gameObject);
        }
        parts.Clear();
        
        tail = null;
        MoveUp();

        gameObject.transform.localEulerAngles = new Vector3(0,0,0); // up
        gameObject.transform.position = new Vector3(0,0,-8);

        ResetMemory();

        partsToAdd = 5;
        addTimer = TIMETOADDBODYPART;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Egg egg = collision.GetComponent<Egg>();
        if (egg)
        {
            Debug.Log("Hit Egg");
            EatEgg(egg);
            int rand = Random.Range(0,3);
            gulpSounds[rand].Play();
        }
        else
        {
            Debug.Log("Hit Obsticle");
            GameController.instance.GameOver();
            dieSound.Play();
        }
    }

    private void EatEgg(Egg egg)
    {
        partsToAdd = 5;
        addTimer   = 0;

        GameController.instance.EggEaten(egg);
    }
}
