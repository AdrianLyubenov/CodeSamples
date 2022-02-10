using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum DayType
{
    WON,
    WON_TODAY,
    NOT_WON,
    INACCESSIBLE
}

public class CalendarDayController : MonoBehaviour
{
    private Color STANDARD_COLOR = Color.white;
    private Color SELECTED_COLOR = new Color32(0xFF, 0xC5, 0x55, 0xFF);
    private Color INACCESSIBLE_COLOR = new Color32(0xFF, 0xFF, 0xFF, 0x00);

    public Text normalText;
    public Text specialText;
    public Image crown;
    public Image superCrown;
    public Image bg;

    private bool selected = false;
    private int dayOfMonth = 1;
    private DayType dayType = DayType.NOT_WON;



    public void Init(CalendarController calendarController)
    {
        GetComponent<Button>().onClick.AddListener( () => {
            if (dayType != DayType.INACCESSIBLE)
            {
                calendarController.SelectDayOfMonth(dayOfMonth);
            }
        } );
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetDayOfMonth(int dayOfMonth)
    {
        this.dayOfMonth = dayOfMonth;
        normalText.text = dayOfMonth.ToString();
        specialText.text = dayOfMonth.ToString();
    }

    public void SetSelected(bool selected)
    {
        this.selected = selected;
        UpdateVisuals();
    }

    public void SetDayType(DayType dayType)
    {
        this.dayType = dayType;
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        crown.transform.DOComplete(true);
        superCrown.transform.DOComplete(true);
        switch(dayType)
        {
            case DayType.WON:
                bg.color = selected ? SELECTED_COLOR : STANDARD_COLOR;
                normalText.gameObject.SetActive(false);
                specialText.gameObject.SetActive(false);
                crown.gameObject.SetActive(true);
                superCrown.gameObject.SetActive(false);
                break;
            case DayType.WON_TODAY:
                bg.color = selected ? SELECTED_COLOR : STANDARD_COLOR;
                normalText.gameObject.SetActive(false);
                specialText.gameObject.SetActive(false);
                crown.gameObject.SetActive(false);
                superCrown.gameObject.SetActive(true);
                break;
            case DayType.NOT_WON:
                bg.color = selected ? SELECTED_COLOR : STANDARD_COLOR;
                normalText.gameObject.SetActive(!selected);
                specialText.gameObject.SetActive(selected);
                crown.gameObject.SetActive(false);
                superCrown.gameObject.SetActive(false);
                break;
            case DayType.INACCESSIBLE:
                bg.color = INACCESSIBLE_COLOR;
                normalText.gameObject.SetActive(true);
                specialText.gameObject.SetActive(false);
                crown.gameObject.SetActive(false);
                superCrown.gameObject.SetActive(false);
                break;
        }
    }

    public void StartFlyCrownAnim(Image startCrown)
    {
        Image crownImg = dayType == DayType.WON_TODAY ? superCrown : crown;
        Vector3 prevPosition = crownImg.transform.position;
        Transform prevParent = crownImg.transform.parent;
        crownImg.transform.SetParent(startCrown.transform);
        crownImg.transform.localPosition = Vector3.zero;
        float scale = startCrown.rectTransform.rect.width / crownImg.rectTransform.rect.width;
        crownImg.transform.localScale = new Vector3(scale, scale, scale);

        crownImg.transform.DOMove(prevPosition, 0.8f);
        crownImg.transform.DOScale(1.0f, 0.2f).SetDelay(0.8f).OnComplete(() => {
            crownImg.transform.SetParent( prevParent );
        });
    }
}
