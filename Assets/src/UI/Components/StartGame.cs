using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartGame : MonoBehaviour
{
    public void GameStarted()
    {
        var list = new List<ReferenceImageInfo>();
        var tex = Resources.Load<Texture2D>("Images/Board/board");
        list.Add(new ReferenceImageInfo(
            tex,
            "Board",
            0.1f
        ));


        tex = Resources.Load<Texture2D>("Images/Cards/Creatures/skeleton");
        list.Add(new ReferenceImageInfo(
            tex,
            "Skeleton",
            0.1f
        ));


        var library = GameObject.FindGameObjectWithTag("Origin").GetComponent<MutableLibrary>();

        library.AddReferenceImages(list);
        library.StartTrackingImages();

        var imgManager = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();

        imgManager.SetTrackedEntities(new List<string> { "Board", "Skeleton" });
    }
}
