using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int attackDamage;
    [SerializeField] private float attackRate;
    [SerializeField] private float attackRange;
    [SerializeField] private float muzzleVelocity;
    [SerializeField] private float rotationSpeed;

    private Vector3 vectorToTarget => targeter.Target.transform.position - transform.position;

    private float attackCharges;

    [ServerCallback]
    void Update()
    {
        bool attackReady = false;
        if (attackCharges > 1 / attackRate)
            attackReady = true;
        else
            attackCharges += Time.deltaTime;

        if (targeter.Target == null)
            return;

        if (!CanFireAtTarget())
            return;

        Quaternion rotationToTarget = Quaternion.LookRotation(vectorToTarget);
        transform.rotation = 
            Quaternion.RotateTowards(transform.rotation, rotationToTarget, rotationSpeed * Time.deltaTime);

        if (attackReady)
        {
            attackCharges = 0;
            Shoot(targeter.Target.AimAtPoint.position);
        }
    }

    [Server]
    private void Shoot(Vector3 target)
    {
        float horizontalAngle = GetAttackAngle(target);
        Debug.Log(nameof(horizontalAngle) + " = " + horizontalAngle);

        Quaternion projectileRotation =
            Quaternion.LookRotation(target - projectileSpawnPoint.position) *
            Quaternion.Euler(Vector3.left * horizontalAngle);

        Projectile projectile =
            Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileRotation);

        projectile.Damage = attackDamage;
        projectile.MuzzleVelocity = muzzleVelocity;

        NetworkServer.Spawn(projectile.gameObject, connectionToClient);
    }

    private float GetAttackAngle(Vector3 target)
    {
        float distance = (projectileSpawnPoint.position - target).magnitude;
        float sin2A = -Physics.gravity.y * distance / (muzzleVelocity * muzzleVelocity);
        return Mathf.Asin(sin2A) * Mathf.Rad2Deg / 2;
    }

    [Server]
    private bool CanFireAtTarget()
    {
        return vectorToTarget.sqrMagnitude <= attackRange * attackRange;
    }
}
