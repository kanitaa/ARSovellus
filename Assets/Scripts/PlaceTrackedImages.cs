using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARTrackedImageManager))]
public class PlaceTrackedImages : MonoBehaviour
{
    private ARTrackedImageManager trackedImageManager;

    [SerializeField]
    GameObject[] ARPrefabs;

    readonly Dictionary<string, GameObject> instantiatedPrefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }
    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        //jos lisätty uusia kuvia näkymään, jotka löytyy reference librarysta, haetaan kuvan nimi ja verrataan luotavaan prefabiin
        //eikä löydy jo instantioituna prefabia niin luodaan kuvan päälle uusi prefab
        foreach(var trackedImage in eventArgs.added)
        {
            var imageName = trackedImage.referenceImage.name;

            foreach(var curPrefab in ARPrefabs)
            {
                if(string.Compare(curPrefab.name, imageName, System.StringComparison.OrdinalIgnoreCase)==0 
                    && !instantiatedPrefabs.ContainsKey(imageName))
                {
                    var newPrefab = Instantiate(curPrefab, trackedImage.transform);
                    instantiatedPrefabs[imageName] = newPrefab;
                }
            }
        }
        //jos olemassa oleviin kuviin tulee muutoksia esim katoaa näkyvistä niin asetataan prefab näkyväksi/näkymättömäksi sen mukaan
        foreach (var trackedImage in eventArgs.updated)
        {
            instantiatedPrefabs[trackedImage.referenceImage.name].SetActive(trackedImage.trackingState == TrackingState.Tracking);

        }
        //jos joku kuva poistettu näkymästä, tuhotaan prefab
        foreach(var trackedImage in eventArgs.removed)
        {
            Destroy(instantiatedPrefabs[trackedImage.referenceImage.name]);
            instantiatedPrefabs.Remove(trackedImage.referenceImage.name);
        }
    }
}
