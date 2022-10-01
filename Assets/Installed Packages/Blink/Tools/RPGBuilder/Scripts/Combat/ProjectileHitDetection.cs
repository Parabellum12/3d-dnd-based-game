using System.Collections.Generic;
using BLINK.RPGBuilder.Combat;
using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.RPGBuilder.LogicMono
{
    public class ProjectileHitDetection : MonoBehaviour
    {
        private CombatEntity casterNode;
        public Rigidbody RB;
        private Transform casterTransform;
        private float curTime;

        private int curNearbyUnitsHit;

        private bool didHitFirstUnit, isReflectingProjectiles = false;
        private int curReflectedProjectileAmount, maxRelfectedProjectileAmount = 0;
        public GameObject curNearbyTargetGO;

        public List<CombatEntity> alreadyHitNodes = new List<CombatEntity>();

        private Vector3 initPos;
        private RPGAbility.RPGAbilityRankData rankREF;
        private int curRankIndex;
        private RPGAbility abREF;

        private Vector3 previousPos;
        private float curVelocity;
        private bool isReady;

        private CombatEntity ownerNode;
        private float cachedExtraSpeed = 0;
        private float cachedMaxDistance = 0;

        public int unitHit;
        private int cachedMaxUnitHit;
        public CombatEntity targettedProjectileTarget;
        public Transform targettedProjectileTargetTransform;

        public void InitProjectile(CombatEntity caster, RPGAbility ab, RPGAbility.RPGAbilityRankData rank)
        {
            casterNode = caster;
            casterTransform = caster.transform;
            initPos = transform.position;
            previousPos = initPos;
            abREF = ab;
            rankREF = rank;
            InitProjectileValues();
            isReady = true;
            Destroy(gameObject, rankREF.projectileDuration);
        }

        void InitProjectileValues()
        {
            cachedMaxUnitHit = rankREF.MaxUnitHit +
                               (int) CombatUtilities.GetTotalOfStatType(casterNode,
                                   RPGStat.STAT_TYPE.ABILITY_MAX_HIT);
            cachedMaxDistance = rankREF.projectileDistance + (rankREF.projectileDistance *
                                                              (CombatUtilities.GetTotalOfStatType(casterNode,
                                                                  RPGStat.STAT_TYPE.PROJECTILE_RANGE) / 100));
            if (RB == null)
            {
                cachedExtraSpeed =
                    CombatUtilities.GetTotalOfStatType(casterNode, RPGStat.STAT_TYPE.PROJECTILE_SPEED);
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (rankREF.projectileDestroyedByEnvironment && CombatManager.Instance.LayerContains(GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers, other.gameObject.layer))
            {
                EnvironmentHit();
            }

            var projREF = other.gameObject.GetComponent<ProjectileHitDetection>();
            if (projREF != null && projREF.isReflectingProjectiles &&
                projREF.curReflectedProjectileAmount < projREF.maxRelfectedProjectileAmount)
            {
                //reflect
                projREF.curReflectedProjectileAmount++;
                RB.velocity = -RB.velocity;
                if (projREF.curReflectedProjectileAmount >= projREF.maxRelfectedProjectileAmount)
                    Destroy(projREF.gameObject);
                return;
            }

            if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE) return;
            CombatEntity nodeREF = other.gameObject.GetComponent<CombatEntity>();
            if (nodeREF == null) return;
            FactionManager.CanHitResult hitResult =
                FactionManager.Instance.AttackerCanHitTarget(rankREF, casterNode, nodeREF);
            if (!hitResult.canHit) return;

            CombatNodeHit(nodeREF);
        }

        private void lookForNextUnit()
        {
            RPGAbility.RPGAbilityRankData rank = casterNode.GetCurrentAbilityRank(abREF, true);
            if (curNearbyUnitsHit < rank.projectileNearbyUnitMaxHit)
            {
                curNearbyUnitsHit++;
                CombatManager.Instance.FIND_NEARBY_UNITS(casterNode, gameObject, abREF, this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void EnvironmentHit()
        {
            if (rankREF.hitEffect != null)
            {
                var hitEffectGO = Instantiate(rankREF.hitEffect, transform.position, Quaternion.identity);
                Destroy(hitEffectGO, rankREF.hitEffectDuration);
            }
            Destroy(gameObject);
        }

        void CombatNodeHit(CombatEntity hitNodeRef)
        {
            if (casterNode != null && hitNodeRef != null)
            {
                if (hitNodeRef.IsDead()) return;
                FactionManager.CanHitResult hitResult =
                    FactionManager.Instance.AttackerCanHitTarget(rankREF, casterNode, hitNodeRef);
                if (hitResult.canHit)
                {
                    unitHit++;
                    CombatManager.Instance.EXECUTE_PROJECTILE_ABILITY_HIT(casterNode, hitNodeRef, rankREF);

                    if (rankREF.hitEffect != null)
                    {
                        CombatManager.Instance.SpawnHitEffect(hitNodeRef, rankREF);
                    }
                    
                    if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        if (unitHit >= cachedMaxUnitHit && !rankREF.isProjectileNearbyUnit)
                        {
                            Destroy(gameObject);
                        }
                    }
                }

                if (!rankREF.isProjectileNearbyUnit) return;
                if (!alreadyHitNodes.Contains(hitNodeRef)) alreadyHitNodes.Add(hitNodeRef);
                if (!didHitFirstUnit)
                {
                    didHitFirstUnit = true;
                    RB.velocity = Vector3.zero;
                }

                lookForNextUnit();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void CheckCollisions()
        {
            curVelocity = Vector3.Distance(previousPos, transform.position);

            var YROT = transform.eulerAngles.y;
            if (YROT < 0)
                YROT = YROT + 365;
            else if (YROT > 360) YROT = YROT - 360;
            Quaternion Rot = Quaternion.AngleAxis(YROT, transform.forward);
            Vector3 Dir = Rot * (transform.forward * curVelocity);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Dir, out hit, curVelocity,
                GameDatabase.Instance.GetCombatSettings().ProjectileDestroyLayers))
            {
                EnvironmentHit();
            }
            else if (Physics.Raycast(transform.position, Dir, out hit, curVelocity))
            {
                CombatEntity nodeREF = hit.collider.gameObject.GetComponent<CombatEntity>();
                if (nodeREF == null) return;
                CombatNodeHit(nodeREF);
            }

            previousPos = transform.position;
        }

        void HandleMovement()
        {
            var transform1 = transform;
            float totalProjectileSpeed = rankREF.projectileSpeed + (rankREF.projectileSpeed * (cachedExtraSpeed / 100));
            transform1.position += transform1.forward * totalProjectileSpeed * Time.deltaTime;
        }

        private void FixedUpdate()
        {
            if (!isReady) return;
            if (cachedMaxDistance != 0)
            {
                if (Vector3.Distance(initPos, transform.position) >= cachedMaxDistance)
                {
                    Destroy(gameObject);
                }
            }

            if (rankREF.useCustomCollision && !rankREF.isProjectileNearbyUnit && rankREF.targetType != RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
            {
                HandleMovement();
                CheckCollisions();
            }

            if (rankREF.targetType == RPGAbility.TARGET_TYPES.TARGET_PROJECTILE)
            {
                HandleTargettedProjectile();
            }

            if (rankREF.isProjectileComeBack)
            {
                HandleComeBack();
            }

            HandleNearbyHit();
        }

        void HandleTargettedProjectile()
        {
            if (targettedProjectileTarget == null) return;
            if (targettedProjectileTarget.IsDead()) Destroy(gameObject);
            if (targettedProjectileTarget == null) Destroy(gameObject);
            Vector3 targetPOS = new Vector3(targettedProjectileTargetTransform.transform.position.x,
                targettedProjectileTargetTransform.transform.position.y,
                targettedProjectileTargetTransform.transform.position.z);
            transform.LookAt(targetPOS);
            transform.position =
                Vector3.MoveTowards(transform.position, targetPOS, rankREF.projectileSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPOS) < 0.25f)
            {
                CombatNodeHit(targettedProjectileTarget);
            }
        }

        void HandleComeBack()
        {
            if (curTime < rankREF.projectileComeBackTime)
            {
                curTime += Time.deltaTime;
            }
            else
            {
                RB.velocity = Vector3.zero;
                Vector3 targetPOS = new Vector3(casterTransform.position.x, casterTransform.position.y + 1.25f,
                    casterTransform.position.z);
                transform.LookAt(targetPOS);
                transform.position = Vector3.MoveTowards(transform.position, targetPOS,
                    rankREF.projectileComeBackSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, targetPOS) < 0.5f) Destroy(gameObject);
            }
        }

        void HandleNearbyHit()
        {
            if (!rankREF.isProjectileNearbyUnit || !didHitFirstUnit || curNearbyTargetGO == null) return;
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(curNearbyTargetGO.transform.position.x, curNearbyTargetGO.transform.position.y + 1f,
                    curNearbyTargetGO.transform.position.z), rankREF.projectileSpeed * Time.deltaTime);
        }

    }
}
