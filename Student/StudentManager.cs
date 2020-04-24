using Assets.Scripts.Dragging;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.Events;

public class StudentManager : MonoBehaviour
{
    [Header("Data")]

    [SerializeField]
    private ItemData itemData;

    [SerializeField]
    private DataManager dataManager;

    [Header("UI elements")]

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TEXDraw tex;

    [Tooltip("DragAndDrop reference on the scene. You can have only one instance or you will have logic problems.")]
    public DragAndDrop dragAndDrop;

    [Tooltip("Toolbox content reference.")]
    public Transform toolboxContent;

    [Tooltip("Answer panel reference.")]
    public Transform answerPanel;

    [Tooltip("Item prefab reference from project folder.")]
    public DragElement itemPrefab;

    private ModalPanel modalPanel;
    private DisplayManager displayManager;

    private UnityAction onWonOk;

    private int currentQuestionIndex = -1;
    private Question currentQuestion = null;

    private int score = 0;

    private void Awake()
    {
        modalPanel = ModalPanel.Instance();
        displayManager = DisplayManager.Instance();

        onWonOk = new UnityAction(OnWonOk);
    }

    public void Start()
    {
        // Load items into the toolbox
        LoadItems();

        // Init random seed
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        // Check the question count
        if (dataManager.questionData.questions.Count == 0)
        {
            displayManager.DisplayMessage("There is no questions!");
        }
        else
        {
            // Init the current question
            PickRandomQuestion();
        }        
    }

    private void LoadItems()
    {
        for (int i = 0; i < itemData.items.Count; i++)
        {
            Item item = itemData.items[i];

            DragElement dragElement = Instantiate(itemPrefab.gameObject, toolboxContent).GetComponent<DragElement>();
            dragElement.typeOfOperator = (i + 1);
            dragElement.GetComponent<Image>().sprite = item.sprite;

        }
    }

    // Attached to DragAndDrop Begin Drag event. Trigger all Between animations forward of empty slots.
    public void AnimateOnlyEmptySlots()
    {
        foreach (DropObject slot in this.dragAndDrop.DropObjectsCache.Values)
        {
            slot.GetComponentInChildren<BetweenColor>().Play(slot.isEmpty);
        }
    }

    // Attached to DragAndDrop End/Drop Drag event. Trigger all Between animations reverse of empty slots.
    public void ReverseAnimationOnlyEmptySlots()
    {
        foreach (DropObject slot in this.dragAndDrop.DropObjectsCache.Values)
        {
            if (slot.isEmpty)
            {
                slot.GetComponentInChildren<BetweenColor>().PlayReverse();
            }
        }
    }

    // Attached to DragAndDrop End Drag event. When new place is invalid return to last place.
    public void OnInvalidPlaceReturn()
    {
        if (this.dragAndDrop.HoveredDropObject == null)
            return;

        if (!this.dragAndDrop.HoveredDropObject.isEmpty)
        {
            BetweenGlobalPosition tweenPosition = this.dragAndDrop.SelectedDragElement.GetComponent<BetweenGlobalPosition>();
            tweenPosition.To = this.dragAndDrop.SelectedDragElement.TransformCache.position;
            tweenPosition.From = this.dragAndDrop.SelectedDragElement.LastPosition;
            tweenPosition.ResetToEnd();
            tweenPosition.OnFinish.AddListener(() =>
            {
                tweenPosition.OnFinish.RemoveAllListeners();
            });

            tweenPosition.PlayReverse();
        }
    }

    private Sprite GetRandomSprite()
    {
        //int index = Random.Range(0, this.spriteAtlas.Sprites.Length);
        //return this.spriteAtlas.Get(index);
        return null;
    }

    private int[] GetCurrentAnwser()
    {
        int childCount = answerPanel.childCount;

        int[] currentAnswer = new int[childCount];
        for (int i = 0; i < childCount; i++)
        {
            currentAnswer[i] = -1;
            Transform slot = answerPanel.GetChild(i);
            DragElement itemElement = slot.GetComponentInChildren<DragElement>();
            if (itemElement != null)
                currentAnswer[i] = itemElement.typeOfOperator;
        }


        return currentAnswer;
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene(Constant.SCENE_MAIN_MENU);
    }

    public void OnSubmitClick()
    {
        
    }

    private void PickRandomQuestion()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, dataManager.questionData.questions.Count);
        } while (dataManager.questionData.questions.Count>1 && currentQuestionIndex == randomIndex);

        currentQuestionIndex = randomIndex;
        currentQuestion = dataManager.questionData.questions[currentQuestionIndex];

        tex.text = currentQuestion.info;

        // Reset answer
        ResetAnwser();
    }

    public void CheckAnswer()
    {
        if (CompareAnswer(GetCurrentAnwser(), currentQuestion.answer))
        {
            // Display correct answer
            displayManager.DisplayMessage("Correct");

            // Increase score
            score += 10;

            scoreText.text = $"Score : {score}";

            // Check win condition
            if (score >= 30)
            {
                modalPanel.Choice("You Won!", onWonOk);
            }
            else
            {
                PickRandomQuestion();
            }
        }
    }

    private bool CompareAnswer(int[] curAnswer, int[] correctAnswer)
    {
        if (curAnswer.Length > correctAnswer.Length)
        {
            return false;   
        }

        for (int i = 0; i < curAnswer.Length; i++)
        {
            if (curAnswer[i] != correctAnswer[i])
                return false;
        }

        return true;
    }

    private void OnWonOk()
    {
        SceneManager.LoadScene(Constant.SCENE_MAIN_MENU);
    }

    private void ResetAnwser()
    {
        int childCount = answerPanel.childCount;

        for (int i = 0; i < childCount; i++)
        {
            DropObject slot = answerPanel.GetChild(i).GetComponent<DropObject>();
            slot.isEmpty = true;
            DragElement dragElement = slot.GetComponentInChildren<DragElement>();
            if (dragElement != null)
                Destroy(dragElement.gameObject);
        }
    }

}

