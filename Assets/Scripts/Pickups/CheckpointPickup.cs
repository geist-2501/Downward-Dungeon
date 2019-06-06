using UnityEngine.SceneManagement;

public class CheckpointPickup : Pickup 
{
    public override void PickupEffects(Player _player)
    {
        optDestroyAfterUse = false;
        optHideAfterUse = false;

        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        //If this is the first time encountering this checkpoint, 
        //update the game master, otherwise, don't do anything.
        if (sceneIndex > GameManager.furthestCheckpointProgress)
        {
            GameManager.UpdateCheckpoint(sceneIndex);
            optShowMessagePopup = true;
        }
        else
        {
            optShowMessagePopup = false;
        }

    }

}
