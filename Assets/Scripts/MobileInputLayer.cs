using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputLayer : MonoBehaviour
{

	Player player;

	[SerializeField] GameObject ev;

	void Start()
	{
		player = FindObjectOfType<Player>();
		EventTrigger trigger = GetComponentInChildren<EventTrigger>();
		EventTrigger.Entry entryDown = new EventTrigger.Entry();
		EventTrigger.Entry entryUp = new EventTrigger.Entry();
		entryDown.eventID = EventTriggerType.PointerDown;
		entryUp.eventID = EventTriggerType.PointerUp;
		entryDown.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
		entryUp.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
		trigger.triggers.Add(entryDown);
		trigger.triggers.Add(entryUp);

		EventSystem _ev = FindObjectOfType<EventSystem>();
		if (_ev == null)
		{
			Instantiate(ev);
		}
	}

	public void OnPointerDownDelegate(PointerEventData data)
	{
		player.AndroidTrigJump();
	}

	public void OnPointerUpDelegate(PointerEventData data)
	{
		player.AndroidReleaseJump();
	}

}
