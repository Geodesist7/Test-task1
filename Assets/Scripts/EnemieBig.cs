using UnityEngine;

public class EnemieBig : Enemie
{
    public GameObject MiniPrefab;
    protected override void Die()
    {
        var offset = Vector3.right * 0.5f;
        for (int i = 0; i < 2; i++)
        {
            Instantiate(MiniPrefab, transform.position + offset, Quaternion.identity);
            offset = -offset;
        }

        base.Die();
    }

}
