using UnityEngine;

public class TankGUI : MonoBehaviour
{
    public TankMoving tankMovement;  

    
    private bool showPanel = true;   
    private float guiSpeed = 10f;    

    
    private Rect panelRect = new Rect(10, 20, 250, 300);
    private Rect toggleButtonRect = new Rect(50, 170, 150, 40);

    void Start()
    {
        
        if (tankMovement == null)
            tankMovement = GetComponent<TankMoving>();

        
        if (tankMovement != null)
            guiSpeed = tankMovement.tankMoveSpeed;  
    }

    void OnGUI()
    {
        
        if (GUI.Button(toggleButtonRect, showPanel ? "Скрыть панель" : "Показать панель"))
        {
            showPanel = !showPanel;
        }

        
        if (!showPanel)
            return;

        
        GUI.Box(panelRect, "Управление танком");

        
        GUILayout.BeginArea(new Rect(panelRect.x + 10, panelRect.y + 30, panelRect.width - 20, panelRect.height - 40));
        {
            GUILayout.Label($"Скорость танка: {guiSpeed:F1}");

            guiSpeed = GUILayout.HorizontalSlider(guiSpeed, 1f, 50f);  

            
            if (tankMovement != null)
            {
                tankMovement.tankMoveSpeed = guiSpeed;  
            }
        }
        GUILayout.EndArea();
    }
}
