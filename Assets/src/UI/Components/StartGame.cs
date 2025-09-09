using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartGame : MonoBehaviour
{
    public void GameStarted()
    {
        var list = new List<ReferenceImageInfo>();
        var tex = Resources.Load<Texture2D>("Images/Board/board_bottom_left");

        list.Add(new ReferenceImageInfo(
            tex,
            "BottomLeft",
            0.1f
        ));

        tex = Resources.Load<Texture2D>("Images/Board/board_bottom_right");
        list.Add(new ReferenceImageInfo(
            tex,
            "BottomRight",
            0.1f
        ));

        tex = Resources.Load<Texture2D>("Images/Board/board_top_left");
        list.Add(new ReferenceImageInfo(
            tex,
            "TopLeft",
            0.1f
        ));

        tex = Resources.Load<Texture2D>("Images/Board/test");
        list.Add(new ReferenceImageInfo(
            tex,
            "TopRight",
            0.1f
        ));



        var library = GameObject.FindGameObjectWithTag("Origin").GetComponent<MutableLibrary>();

        library.AddReferenceImages(list);
        library.StartTrackingImages();

        var imgManager = GameObject.FindGameObjectWithTag("Origin").GetComponent<MultipleImageTrackingManager>();

        imgManager.SetTrackedEntities(new List<string> { "BottomLeft", "BottomRight", "TopLeft", "TopRight" });
    }
}
