using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LightsMenu : MonoBehaviour {

    public GameObject m_lightTogglePrefab;
    public MenuColors m_changeColors;

    private Light[] m_lights;
    private Toggle[] m_lightsToggle;

    void Start () {
        m_lights = FindObjectsOfType(typeof(Light)) as Light[];
        m_lightsToggle = new Toggle[m_lights.Length];
        for(int i = 0; i < m_lights.Length; ++i) {
            Light light = m_lights[i];
            GameObject lightToggle = GameObject.Instantiate(m_lightTogglePrefab) as GameObject;
            lightToggle.transform.SetParent(transform, false);
            Text label = lightToggle.GetComponentInChildren<Text>() as Text;
            label.text = light.name;
            m_lightsToggle[i] = lightToggle.GetComponent<Toggle>() as Toggle;
            UnityEngine.Events.UnityAction<bool> action = delegate(bool e)
            {
                light.enabled = e;
            };
            m_lightsToggle[i].onValueChanged.AddListener(action);
        }
        if(m_changeColors) {
            m_changeColors.handleGameObject(gameObject);
        }
    }

    void Update()
    {
        for(int i = 0; i < m_lights.Length; ++i) {
            if(m_lights[i].enabled != m_lightsToggle[i].isOn) {
                m_lightsToggle[i].isOn = m_lights[i].enabled;
            }
        }
    }

}
