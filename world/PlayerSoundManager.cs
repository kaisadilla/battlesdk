namespace battlesdk.world;
public class PlayerSoundManager {
    private const float COLLISION_CD = 0.5f;

    private float _collisionTimestamp = float.MinValue;

    public void PlayCollision () {
        if (Time.TotalTime - _collisionTimestamp >= COLLISION_CD) {
            _collisionTimestamp = Time.TotalTime;
            Audio.PlaySound("collision.wav");
        }
    }
}
