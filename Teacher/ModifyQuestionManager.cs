using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using Assets.Scripts.Dragging;
using UnityEngine.Events;

public class ModifyQuestionManager : MonoBehaviour
{
    [Header("Data")]

    [SerializeField]
    private ItemData itemData;

    [SerializeField]
    private DataManager dataManager;
        
    [Header("UI Elements")]
    [SerializeField]
    public TEXDraw tex;

    [Tooltip("Answer panel reference.")]
    public Transform answerPanel;

    [Tooltip("Question index reference.")]
    public TextMeshProUGUI questionIndexText;

    [Tooltip("Item prefab reference from project folder.")]
    public DragElement itemPrefab;

    private ModalPanel modalPanel;

    private DisplayManager displayManager;

    private int currentQuestionIndex = -1;

    private UnityAction onDeleteYes;
    private UnityAction onDeleteNo;
    private UnityAction onEditYes;
    private UnityAction onEditNo;

    private void Awake()
    {
        modalPanel = ModalPanel.Instance();
        displayManager = DisplayManager.Instance();

        onDeleteYes = new UnityAction(OnDeleteYes);
        onDeleteNo = new UnityAction(OnDeleteNo);
        onEditYes = new UnityAction(OnEditYes);
        onEditNo = new UnityAction(OnEditNo);
    }

    private void Start()
    {
        // Check if there is editing question
        if (Constant.EditingQuestionIndex >= 0)
        {
            currentQuestionIndex = Constant.EditingQuestionIndex;
            Constant.EditingQuestionIndex = -1;

            displayManager.DisplayMessage("Updated the question successfully!");
        }
        else
        {
            currentQuestionIndex = 0;
        }


        DisplayCurrentQuestion(true);
    }

    public void OnBackClick()
    {
        SceneManager.LoadScene(Constant.SCENE_TEACHER);
    }

    public void OnNextClick()
    {
        // Increase current question index
        currentQuestionIndex++;

        DisplayCurrentQuestion(false);
    }

    public void OnEditClick()
    {
        if (string.IsNullOrEmpty(questionIndexText.text))
        {
            displayManager.DisplayMessage("Please click 'Next' button to start!");
            return;
        }
        modalPanel.Choice("Would you like to edit this question?", onEditYes, onEditNo);
    }

    public void OnDeleteClick()
    {
        if (string.IsNullOrEmpty(questionIndexText.text))
        {
            displayManager.DisplayMessage("Please click 'Next' button to start!");
            return;
        }
        modalPanel.Choice("Are you sure to delete this question?", onDeleteYes, onDeleteNo);
    }

    private void DisplayCurrentQuestion(bool isStartup)
    {
        int questionCount = dataManager.questionData.questions.Count;
        if (currentQuestionIndex >= questionCount)
            currentQuestionIndex = 0;

        // Display question index
        questionIndexText.text = $"{currentQuestionIndex + 1}/{questionCount}";

        // Display question
        Question question = dataManager.questionData.questions[currentQuestionIndex];

        tex.text = question.info;

        ResetAnwser();

        DisplayAnswer(question.answer, isStartup);
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

            dragElement.isClonning = false;
            dragElement.GetComponent<Graphic>().raycastTarget = false;
        }
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

    void OnDeleteYes()
    {
        // Delete question
        dataManager.questionData.questions.RemoveAt(currentQuestionIndex);
        dataManager.SaveQuestionData();

        // Display current question
        DisplayCurrentQuestion(false);
    }

    void OnDeleteNo()
    {

    }

    void OnEditYes()
    {
        Constant.EditingQuestionIndex = currentQuestionIndex;
        SceneManager.LoadScene(Constant.SCENE_TEACHER_ADD_QUESTION);
    }

    void OnEditNo()
    {

    }
}

