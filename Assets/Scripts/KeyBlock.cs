﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBlock : MonoBehaviour {

	private Animator anim;
	private Collider2D col;
	[SerializeField] AnimationClip destroyAnim;
	
	public void UnlockAndDestroy()
	{
		anim = GetComponent<Animator>();
		anim.SetBool("deactivate", true);
		col = GetComponent<Collider2D>();
		col.enabled = false;
		Destroy(gameObject, destroyAnim.length);
	}

	public void Show()
	{
		anim = GetComponent<Animator>();
		anim.SetBool("deactivate", false);
		col = GetComponent<Collider2D>();
		col.enabled = true;
	}

	public void Hide()
	{
		anim = GetComponent<Animator>();
		anim.SetBool("deactivate", true);
		col = GetComponent<Collider2D>();
		col.enabled = false;
	}
}
