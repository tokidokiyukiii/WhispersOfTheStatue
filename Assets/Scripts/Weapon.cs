using UnityEngine;

public class Weapon : MonoBehaviour
{
    public string weaponName;
    public float damage = 1;
    public enum WeaponType { Melee, Bullet  };
    public WeaponType type;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null )
        {
            enemy.TakeDamage(damage);
            if(type == WeaponType.Bullet)
            {
                Destroy(gameObject);
            }
        }
    }
}
