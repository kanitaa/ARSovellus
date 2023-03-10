using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.HID;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    //Variables for spawning and moving objects
    [SerializeField]
    GameObject [] objectToSpawn;
    [SerializeField]
    List<GameObject> spawnedObject = new List<GameObject>();
    ARRaycastManager rcManager;
    Vector2 touchPosition;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    [SerializeField]
    int listIndex=0;
    int spawnIndex=0;
    
  
    Animator catAnim;

    //Game over variables
    [SerializeField]
    TextMeshProUGUI scoreCounterText;
    int scoreCounter;
    [SerializeField]
    GameObject gameEndPanel;

    //Audio variables
    AudioSource audioS;
    [SerializeField]
    AudioClip buttonClip, winClip;



    //Variables for changing appearances
    [SerializeField]
    Material[] catMaterials;

    [SerializeField]
    Material[] ballMaterials;
    int catMaterialIndex = 0;
    int ballMaterialIndex = 0;
    int prevListIndex=0;
    [SerializeField]
    Button leftButton, rightButton;
    public bool cat;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        rcManager = GetComponent<ARRaycastManager>();
        audioS = GetComponent<AudioSource>();
    }
    private void Start()
    {
        leftButton.onClick.AddListener(ChangeMaterialLeft);
        rightButton.onClick.AddListener(ChangeMaterialRight);
    }

    //check if player is touching screen
    bool IsTouched(out Vector2 touchPos)
    {
        if (Input.touchCount > 0)
        {
            touchPos = Input.GetTouch(0).position;
            return true;
        }
       
       touchPos = default;
        //set cats animations and indexes
        if (listIndex != -1 && spawnedObject[listIndex] != null && spawnedObject[listIndex].GetComponent<Cat>() != null && catAnim.GetBool("Walk"))
        {
            catAnim.SetBool("Walk", false); 
            catAnim.SetBool("Sit", true);
            prevListIndex = listIndex;
            listIndex = -1;
        }
        else if (listIndex != -1 && spawnedObject[listIndex] != null && spawnedObject[listIndex].GetComponent<Ball>() != null)
        {
            prevListIndex = listIndex;
            listIndex = -1;
        }

        return false; 
        
    }
    private void Update()
    {
         //if player doesnt touch screen, return   
        if (!IsTouched(out Vector2 touchPosition)) return;
       
        //raycast on plane that was created with ar plane manager
        if (listIndex!=-1 && rcManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
          
            var hitPose = hits[0].pose;
            //if object is new, create it
            if (spawnedObject[listIndex] == null)
            {
                spawnedObject[listIndex] = Instantiate(objectToSpawn[spawnIndex], hitPose.position, objectToSpawn[spawnIndex].transform.rotation);
                //check if created object is cat or ball and set their index for later use
                if (spawnedObject[listIndex].GetComponent<Cat>() != null)
                {
                    catAnim = spawnedObject[listIndex].GetComponent<Animator>();
                    spawnedObject[listIndex].GetComponent<Cat>().SetIndex(listIndex);

                }else if(spawnedObject[listIndex].GetComponent<Ball>() != null)
                {
                    spawnedObject[listIndex].GetComponent<Ball>().SetIndex(listIndex);
                }
                spawnedObject.Add(null); //increase list size by one
            }
            else
            {
                //if there is already object move it, if its cat set animation
                if (spawnedObject[listIndex].GetComponent<Cat>() != null && !catAnim.GetBool("Walk"))
                {
                    catAnim.SetBool("Sit", false);
                    catAnim.SetBool("Walk", true);

                }
                spawnedObject[listIndex].transform.position = hitPose.position;
                //set rotation for object that is being moved
                if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved && spawnedObject[listIndex] != null)
                {
                    float rotationSpeed = 0.5f;
                    Vector2 touchDelta = Input.GetTouch(0).deltaPosition;
                    Vector3 targetPosition = new Vector3(touchDelta.x, 0, touchDelta.y) * rotationSpeed;
                    Quaternion targetRotation = Quaternion.LookRotation(targetPosition, Vector3.up);
                    spawnedObject[listIndex].transform.rotation = targetRotation;
                }
            }
        }
      
    }

    //index of instantiated objects
    public void SetListIndex(int listIndex)
    {
        this.listIndex = listIndex;
    }
    //set reference to animator each time new cat is chosen
    public void SetAnimator(Animator catAnim)
    {
        this.catAnim = catAnim;
    }

    //set index values when creating new object
    public void SetSpawn(int spawnIndex)
    {
        this.spawnIndex = spawnIndex;

        if(spawnedObject.Count() - listIndex !=1)
        {
            listIndex = spawnedObject.Count()-1;
        }
        audioS.PlayOneShot(buttonClip);
    }

    
    public void ChangeMaterialLeft()
    {
        //check if previous object was cat or ball and change its material
        if (spawnedObject[prevListIndex].GetComponent<Cat>() != null && cat)
        {
            if(catMaterialIndex == 0) catMaterialIndex = catMaterials.Length - 1;
            else  catMaterialIndex--;
            spawnedObject[prevListIndex].GetComponentInChildren<SkinnedMeshRenderer>().material = catMaterials[catMaterialIndex];
        }
        if (spawnedObject[prevListIndex].GetComponent<Ball>() != null &&!cat)
        {
            if (ballMaterialIndex == 0) ballMaterialIndex = ballMaterials.Length - 1;
            else ballMaterialIndex--;
            spawnedObject[prevListIndex].GetComponentInChildren<MeshRenderer>().material = ballMaterials[ballMaterialIndex];
        }
        audioS.PlayOneShot(buttonClip);
    }

    public void ChangeMaterialRight()
    {
        if (spawnedObject[prevListIndex].GetComponent<Cat>() != null && cat)
        {
            if (catMaterialIndex == catMaterials.Length - 1) catMaterialIndex = 0;
            else catMaterialIndex++;
            spawnedObject[prevListIndex].GetComponentInChildren<SkinnedMeshRenderer>().material = catMaterials[catMaterialIndex];
        }
        if (spawnedObject[prevListIndex].GetComponent<Ball>() != null &&!cat)
        {
            if (ballMaterialIndex == ballMaterials.Length - 1) ballMaterialIndex = 0;
            else ballMaterialIndex++;
            spawnedObject[prevListIndex].GetComponentInChildren<MeshRenderer>().material = ballMaterials[ballMaterialIndex];
        }
        audioS.PlayOneShot(buttonClip);
    }

    public void IncreaseScoreCounter()
    {
        scoreCounter++;
        scoreCounterText.text = "Balls kicked: "+scoreCounter.ToString() + "/10";

        if (scoreCounter == 10)
        {
            gameEndPanel.SetActive(true);
            audioS.PlayOneShot(winClip);
        }

    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
 