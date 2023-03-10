using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cat : MonoBehaviour
{

    int index;
    Animator catAnim;
    AudioSource audioS;
    [SerializeField]
    AudioClip meowClip, ballKickClip;
  
    private void Start()
    {
        catAnim = GetComponent<Animator>();
        audioS = GetComponent<AudioSource>();
    }
    public void SetIndex(int _index)
    {
        index = _index;
    }
   
    private void OnMouseDown()
    {
        //tell gamemanager info about this cat when its tapped with finger
        GameManager.instance.SetListIndex(index);
        GameManager.instance.SetAnimator(catAnim);
        GameManager.instance.cat = true;
        float random = Random.Range(0.75f, 1.25f);
        audioS.pitch = random;
        audioS.PlayOneShot(meowClip);
    }
   
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            float random = Random.Range(0.75f, 1.25f);
            audioS.pitch = random;
            audioS.PlayOneShot(ballKickClip);
            GameManager.instance.IncreaseScoreCounter();
            collision.gameObject.GetComponent<Rigidbody>().AddForce(this.transform.forward * 0.4f, ForceMode.Impulse);
        }
       
    }
}
