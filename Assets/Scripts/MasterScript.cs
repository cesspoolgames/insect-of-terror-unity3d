﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MasterScript : MonoBehaviour
{

    public GameObject enemyPrefab;
    public CameraScript camera_script;

    public float left, top, right, bottom;
    public float enemy_rotation_speed;
    public float enemy_speed;
    public float create_enemy_delay = 0.7f;
    public int max_enemies = 30;

    public Text score_text;
    public Text countdown_text;
    private int score;
    public int score_max = 500;
    public int countdown_start = 30;
    private int countdown;
    private int my_level;

    private bool finish_started = false;

    // Use this for initialization
    void Start()
    {
        left = InterestingGameStuff.left;
        top = InterestingGameStuff.top;
        right = InterestingGameStuff.right;
        bottom = InterestingGameStuff.bottom;

        InvokeRepeating("RotateEnemies", 0, 0.02f);
        InvokeRepeating("CreateRandomEnemy", 0, create_enemy_delay);
        UpdateScoreText();
        countdown = countdown_start;
        UpdateCountdownText();
        InvokeRepeating("CountdownBeat", 1.0f, 1.0f);
        my_level = InterestingGameStuff.level;

        score_max += (score_max / 10) * my_level;
        UpdateScoreText();

        //CreateLevelBorders();

        StartCoroutine(camera_script.TwirlDown());

    }

    //void CreateLevelBorders()
    //{
    //    GameObject tilePrefab = Resources.Load<GameObject>("Prefabs/Border Tile");
    //    SpriteRenderer spriteRenderer = tilePrefab.GetComponent<SpriteRenderer>();

    //    float margin_y = (InterestingGameStuff.bottom - InterestingGameStuff.top) / 12; 
    //    float margin_x = (InterestingGameStuff.right - InterestingGameStuff.left) / 16;
    //    float z = +4;

    //    // vertical columns
    //    for (float y = InterestingGameStuff.top; y <= InterestingGameStuff.bottom; y += margin_y)
    //    {
    //        Vector3 position;
    //        position = new Vector3(InterestingGameStuff.left, y, z);
    //        Instantiate(tilePrefab).transform.position = position;
    //        position = new Vector3(InterestingGameStuff.right, y, z);
    //        Instantiate(tilePrefab).transform.position = position;
    //    }
    //    // horizontal columns
    //    for (float x = InterestingGameStuff.left; x <= InterestingGameStuff.right; x += margin_x)
    //    {
    //        Vector3 position;
    //        position = new Vector3(x,InterestingGameStuff.top, z);
    //        Instantiate(tilePrefab).transform.position = position;
    //        position = new Vector3(x, InterestingGameStuff.bottom, z);
    //        Instantiate(tilePrefab).transform.position = position;
    //    }
    //}

    /// <summary>
    /// Iterates through enemies and changes their rotationVector,
    /// for an awesome wiggly moving effect.
    /// </summary>
    void RotateEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyScript script = enemy.GetComponent<EnemyScript>();
            if (script)
            { // for some reason it was null on game very start, so ignore if null
                script.randomRotationVector();
            }
        }
    }

    void CreateRandomEnemy()
    {
        // limit active enemies number
        int counter = 0;
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyScript script = obj.GetComponent<EnemyScript>();
            if (!script || script.IsShitted()) continue;
            counter++;
        }
        if (counter >= max_enemies)
        {
            return;
        }

        Vector3 new_pos = Vector3.zero;
        switch (Random.Range(0, 4))
        {
            // position in one of edges of screen
            case 0:
                new_pos.x = left;
                new_pos.y = top + (bottom - top) * Random.value;
                break;
            case 1:
                new_pos.x = right;
                new_pos.y = top + (bottom - top) * Random.value;
                break;
            case 2:
                new_pos.x = left + (right - left) * Random.value;
                new_pos.y = top;
                break;
            case 3:
                new_pos.x = left + (right - left) * Random.value;
                new_pos.y = bottom;
                break;
        }

        GameObject new_enemy = Instantiate(enemyPrefab, new_pos, Quaternion.identity) as GameObject;
        // call Initialize
        new_enemy.GetComponent<EnemyScript>().Initialize(my_level, enemy_rotation_speed, enemy_speed, left, top, right, bottom);
        new_enemy.transform.Rotate(new Vector3(0, 0, Random.value * 360));
    }

    void ScoreUp(int howmuch = 1)
    {
        score += howmuch;
        UpdateScoreText();

        // end level?
        if (score >= score_max)
        {
            StartCoroutine("FinishLevel");
        }
    }

    void UpdateScoreText()
    {
        score_text.text = "Score: " + score + "/" + score_max;
    }

    void UpdateCountdownText()
    {
        countdown_text.text = "Time left: " + countdown;
    }

    void CountdownBeat()
    {
        countdown--;
        if (countdown <= 0)
        {
            countdown = 0;
            // level over
        }
        if (countdown <= 10)
        {
            // warning!
            countdown_text.GetComponent<TextColorScript>().target_color = Color.red;
        }
        UpdateCountdownText();
    }

    // note! I had a bug when this was in `Start` (where it shouldn't be). Probably `twirl` on camera_script was not yet set up.
    public IEnumerator FinishLevel()
    {
        // collect 'bunos' time
        // show cool stuff
        // only do this once
        if (!finish_started)
        {
            finish_started = true;
            yield return StartCoroutine(camera_script.TwirlUp());
            yield return StartCoroutine("_FinalAction");
        }
    }

    // Final commands to invoke when finishing level, called in `FinishLevel`
    IEnumerator _FinalAction()
    {
        Application.LoadLevel("boss");
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
