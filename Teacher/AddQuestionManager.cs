using Assets.Scripts.Dragging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class AddQuestionManager : MonoBehaviour
{
    [Header("Data")]

    [SerializeField]
    private ItemData itemData;

    [SerializeField]
    private DataManager dataManager;

    [Header("UI Elements")]
    [SerializeField]
    private TextMeshProUGUI title;

    [SerializeField]
    private Button backButton;

    [SerializeField]
    private Button submitButton;

    [SerializeField]
    private InputField questionInput;

    [SerializeField]
    public TEXDraw tex;

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

    private void Awake()
    {
        modalPanel = ModalPanel.Instance();
        displayManager = DisplayManager.Instance();
    }

    public void Start()
    {
        // Clear the question input
        OnEquationValueChanged("");

        // Load items into the toolbox
        LoadItems();

        // Load editing question if available
        if (Constant.EditingQuestionIndex >= 0)
        {
            DisplayEditingQuestion();

            title.text = "Update question";
            backButton.gameObject.SetActive(false);
            submitButton.GetComponentInChildren<TextMeshProUGUI>().text = "Update";
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

    public void OnEquationValueChanged(string str)
    {
        tex.text = str;
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene(Constant.SCENE_TEACHER);
    }

    public void OnSubmitClick()
    {
        string questionText = questionInput.text;
        if (string.IsNullOrEmpty(questionText))
        {
            displayManager.DisplayMessage("Please input the equation!");
            return;
        }            

        // Add a new question
        if (Constant.EditingQuestionIndex < 0)
        {
            Question question = new Question();
            question.info = questionText;
            question.answer = GetCurrentAnwser();

            dataManager.questionData.questions.Add(question);
            dataManager.SaveQuestionData();

            // Show Message box
            ModalPanel modalPanel = ModalPanel.Instance();
            modalPanel.Choice("The question is successfully added!");

            // Clear the equation input
            questionInput.text = "";
            ResetAnwser();
        }
        // Update the question
        else
        {
            Question question = dataManager.questionData.questions[Constant.EditingQuestionIndex];
            question.info = questionText;
            question.answer = GetCurrentAnwser();
            dataManager.SaveQuestionData();

            SceneManager.LoadScene(Constant.SCENE_TEACHER_MODIFY_QUESTION);
        }
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

    // For editing questions
    private void DisplayEditingQuestion()
    {
        // Display question
        Question question = dataManager.questionData.questions[Constant.EditingQuestionIndex];

        questionInput.text = question.info;
        tex.text = question.info;

        ResetAnwser();

        DisplayAnswer(question.answer, true);
    }

    private void DisplayAnswer(int[] answer, bool isStartup)
    {
        int childCount = answerPanel.childCount;
        int answerCount = answer.Length;

        int count = Math.Min(childCount, answerCount);

        for (int i = 0; i < count; i++)
        {
            int typeOfOperator = (answer[i] - 1);
            if (typeOfOperator < 0 || typeOfOperator >= itemData.items.Count)
                continue;

            DropObject dropObject = answerPanel.GetChild(i).GetComponent<DropObject>();

            DragElement dragElement = Instantiate(itemPrefab, dropObject.TransformCache).GetComponent<DragElement>();

            Item item = itemData.items[typeOfOperator];
            dragElement.typeOfOperator = typeOfOperator;
            dragElement.GetComponent<Image>().sprite = item.sprite;

            dropObject.isEmpty = false;

            if (isStartup)
            {
                dragElement.TransformCache.localPosition = new Vector2(50, -50);
            }
            else
            {
                dragElement.TransformCache.localPosition = Vector2.zero;
            }

            dragElement.cloneOnDragging = false;
            dragElement.isClonning = false;
            dragElement.lastDropObject = dropObject;
            //dragElement.GetComponent<Graphic>().raycastTarget = false;
        }
    }
}
