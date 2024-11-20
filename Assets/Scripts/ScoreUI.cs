using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public ScoreController scoreController;
    public ControllerInput controllerInput;
    public NoteSpawner noteSpawner;
    public NoteStrummer noteStrummer;
    public Text scoreText;
    public Text highscoreText;
    public Text multiplierText;
    public Color[] multiplierColours;
    public Color[] starPowerColours;
    public Image starPowerBar;
    public Image fretboardStar;
    public GameObject highscoreMessage;
    private int ticksPerBeat;
    private int liveScore = 0;
    private int highscore = 0;
    private int baseMultiplier = 1;
    private int multiplierCount = 0;
    private bool starPower = false;
    private bool starPowerPhrase = false;
    private bool starPowerMissed = false;
    private float starPowerCount = 0;
    private float starPowerDecrement = 0.02f;

    private void OnEnable() {
        controllerInput.SelectPressed += StarPowerActivate;
    }

    private void OnDisable() {
        controllerInput.SelectPressed -= StarPowerActivate;
    }

    public void ResetScore(int ticksPerBeat) {
        this.ticksPerBeat = ticksPerBeat;
        highscore = scoreController.GetHighscore();
        highscoreText.text = highscore.ToString();
        liveScore = 0;
        highscoreMessage.SetActive(false);
        starPowerCount = 0;
        starPower = false;
        starPowerPhrase = false;
        starPowerMissed = false;
        starPowerBar.fillAmount = 0;
        fretboardStar.enabled = false;
        UpdateUI();
    }

    public void ScoreNote(bool isStar) {
        if (isStar) {
            StarPowerStart();
        } else {
            StarPowerEnd();
        }
        liveScore += 50 * baseMultiplier * (starPower ? 2 : 1);
        if (baseMultiplier < 4) {
            multiplierCount++;
            if (multiplierCount >= 10) {
                multiplierCount = 0;
                baseMultiplier += 1;
            }
        }
        UpdateUI();
    }

    public void ScoreSustain(int startTicks, int endTicks) {
        float beats = ((endTicks - startTicks) / (float)ticksPerBeat);
        int beatsInt = (int)beats;
        if (beats - beatsInt > 0.7f) {
            beatsInt += 1;
        }
        liveScore += 25 * beatsInt * baseMultiplier * (starPower ? 2 : 1);
        UpdateUI();
    }

    public void ResetCombo() {
        baseMultiplier = 1;
        multiplierCount = 0;
        UpdateUI();
    }

    public void SaveScore() {
        scoreController.UpdateScore(liveScore);
    }

    private void StarPowerStart() {
        if (!starPowerPhrase) {
            starPowerPhrase = true;

        }
    }

    public void StarPowerMiss() {
        if (starPowerPhrase) {
            starPowerMissed = true;
        }
    }

    private void StarPowerEnd() {
        if (starPowerPhrase) {
            if (!starPowerMissed) {
                // Add 25% to starpowermeter
                starPowerCount = Mathf.Clamp01(starPowerCount + 0.25f);
                starPowerBar.fillAmount = starPowerCount;
                if (starPowerCount >= 0.5f) {
                    starPowerBar.color = starPowerColours[1];
                }
            } else {
                starPowerMissed = false;
            }
            starPowerPhrase = false;
        }
    }

    private void StarPowerActivate() {
        if (starPowerCount >= 0.5f) {
            if (!starPower) {
                starPower = true;
                starPowerDecrement = 0.01f / noteSpawner.GetBarLength();
                noteStrummer.SetStarState(true);
                UpdateUI();
            }
        }
    }

    public void WhammyStarSustain() {
        // Add small amount to starpowermeter
        starPowerCount = Mathf.Clamp01(starPowerCount + 0.005f);
        starPowerBar.fillAmount = starPowerCount;
        if (starPowerCount >= 0.5f) {
            starPowerBar.color = starPowerColours[1];
        }
    }

    private void FixedUpdate() {
        if (starPower && starPowerCount > 0) {
            starPowerCount -= starPowerDecrement;
            if (starPowerCount <= 0) {
                starPowerCount = 0;
                starPower = false;
                starPowerBar.fillAmount = 0;
                starPowerBar.color = starPowerColours[0];
                noteStrummer.SetStarState(false);
                UpdateUI();
            } else {
                starPowerBar.fillAmount = starPowerCount;
            }
        }
    }

    private void UpdateUI() {
        scoreText.text = liveScore.ToString();
        if (liveScore > highscore) {
            highscoreMessage.SetActive(true);
        }
        fretboardStar.enabled = starPower;
        if (starPower) {
            multiplierText.text = (baseMultiplier * 2).ToString();
            multiplierText.color = starPowerColours[1];
        } else {
            multiplierText.color = multiplierColours[baseMultiplier - 1];
            if (baseMultiplier == 1) {
                multiplierText.text = "";
            } else {
                multiplierText.text = baseMultiplier.ToString();
            }
        }
    }
}