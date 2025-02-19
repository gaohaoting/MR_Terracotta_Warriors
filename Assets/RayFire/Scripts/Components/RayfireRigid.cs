﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid")]
    [HelpURL ("https://rayfirestudios.com/unity-online-help/components/unity-rigid-component/")]
    public class RayfireRigid : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }

        // UI
        public InitType              initialization      = InitType.ByMethod;
        public SimType               simulationType      = SimType.Dynamic;
        public ObjectType            objectType          = ObjectType.Mesh;
        public DemolitionType        demolitionType      = DemolitionType.None;
        public RFPhysic              physics             = new RFPhysic();
        public RFActivation          activation          = new RFActivation();
        public RFLimitations         limitations         = new RFLimitations();
        public RFDemolitionMesh      meshDemolition      = new RFDemolitionMesh();
        public RFDemolitionCluster   clusterDemolition   = new RFDemolitionCluster();
        public RFReferenceDemolition referenceDemolition = new RFReferenceDemolition();
        public RFSurface             materials           = new RFSurface();
        public RFDamage              damage              = new RFDamage();
        public RFFade                fading              = new RFFade();
        public RFReset               reset               = new RFReset();
        
        // Hidden
        public bool                initialized;
        public RFMesh[]            rfMeshes;
        public List<RayfireRigid>  fragments;
        public Quaternion          cacheRotation; // NOTE. Should be public, otherwise rotation error on demolition.
        public Transform           transForm;
        public Transform           rootChild;
        public Transform           rootParent;
        public MeshFilter          meshFilter;
        public MeshRenderer        meshRenderer;
        public SkinnedMeshRenderer skr;
        public RayfireRestriction  rest;
        public RayfireSound        sound;
       
        // Non Serialized
        [NonSerialized] public bool                corState;
        [NonSerialized] public List<Transform>     particleList;
        [NonSerialized] public List<RayfireDebris> debrisList;
        [NonSerialized] public List<RayfireDust>   dustList;
        [NonSerialized] public RFDictionary[]      subIds;
        [NonSerialized] public Vector3[]           pivots;
        [NonSerialized] public Mesh[]              meshes;
        [NonSerialized] public RayfireRigid        meshRoot;
        [NonSerialized] public RayfireRigidRoot    rigidRoot;
        [NonSerialized] public int                 debrisState = 1; // 1 - debrisList have  to be collected at Initialize
        [NonSerialized] public int                 dustState = 1;   // 0 - dustList already set by other object, skip collecting
        
        // Events
        public RFDemolitionEvent  demolitionEvent  = new RFDemolitionEvent();
        public RFActivationEvent  activationEvent  = new RFActivationEvent();
        public RFRestrictionEvent restrictionEvent = new RFRestrictionEvent();

        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        // Awake
        void Awake()
        {
            // Awake Mesh input
            MeshInput();
            
            // Initialize at start
            if (initialization == InitType.AtStart)
                Initialize();
        }
        
        // Initialize 
        public void Initialize()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Not initialized
            if (initialized == false)
            {
                // Init Awake methods
                AwakeMethods();

                // Init sound
                RFSound.InitializationSound(sound, limitations.bboxSize);
            }
            
            // TODO add reinit for already initialized objects in case of property change
        }
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();

            // Set components for mesh / skinned mesh / clusters
            SetComponentsBasic();

            // Set particles
            RFPoolingParticles.InitializeParticles(this);
            
            // Init mesh root.
            if (SetupMeshRoot() == true)
                return;
            
            // Check for user mistakes
            RFLimitations.Checks(this);
            
            // Set components for mesh / skinned mesh / clusters
            SetComponentsPhysics();

            // Initialization Mesh input
            if (meshDemolition.inp == RFDemolitionMesh.MeshInputType.AtInitialization)
                MeshInput();
            
            // Precache meshes at awake
            RFDemolitionMesh.Awake(this);

            // Skinned mesh
            SetSkinnedMesh();

            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Set Start variables
            SetObjectType();
            
            // Runtime ops
            if (Application.isPlaying == true)
            {
                // Start all coroutines
                StartAllCoroutines();

                // Object initialized
                initialized = true;
            }
        }

        // Set skinned mesh
        void SetSkinnedMesh()
        {
            // Skinned mesh FIXME
            if (objectType == ObjectType.SkinnedMesh)
            {
                // Reset rigid data
                Default();

                // Check for demolition state every frame
                if (demolitionType != DemolitionType.None)
                    StartCoroutine (limitations.DemolishableCor(this));
                
                // Reset rigid data
                Default();

                // Set physics properties
                physics.destructible = physics.Destructible;
                
                if (Application.isPlaying == true)
                    initialized = true;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Enable/Disable
        /// /////////////////////////////////////////////////////////
        
        // Disable
        void OnDisable()
        {
            // Set coroutines states
            corState                         = false;
            limitations.dmlCorState          = false;
            physics.physicsDataCorState      = false;
            activation.inactiveCorState      = false;
            activation.velocityCorState      = false;
            activation.offsetCorState        = false;
            fading.offsetCorState            = false;
        }

        // Activation
        void OnEnable()
        {
            // Start cors // TODO add support for fragment caching and the rest cors:skinned
            if (gameObject.activeSelf == true && initialized == true && corState == false)
            {
                StartAllCoroutines();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Setup
        /// /////////////////////////////////////////////////////////

        // Editor Setup
        public void EditorSetup()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Setup mesh root
            if (objectType == ObjectType.MeshRoot)
                EditorSetupMeshRoot();

            // Setup clusters
            if (objectType == ObjectType.ConnectedCluster || objectType == ObjectType.NestedCluster)
                RFDemolitionCluster.ClusterizeEditor (this);
        }
        
        // Editor Reset
        public void ResetSetup()
        {
            // Deactivated
            if (gameObject.activeSelf == false)
                return;
            
            // Reset setup for mesh root
            if (objectType == ObjectType.MeshRoot)
                ResetMeshRootSetup();
            
            // Reset Setup for clusters 
            if (objectType == ObjectType.ConnectedCluster || objectType == ObjectType.NestedCluster)
                RFDemolitionCluster.ResetClusterize (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Awake ops
        /// /////////////////////////////////////////////////////////
        
        // Define basic components
        public void SetComponentsBasic()
        {
            // Set shatter component
            meshDemolition.sht = meshDemolition.use == true 
                ? GetComponent<RayfireShatter>() 
                : null;
            
            // Tm
            transForm = GetComponent<Transform>();
            
            // Mesh/Renderer components
            if (objectType == ObjectType.Mesh)
            {
                meshFilter   = GetComponent<MeshFilter>();
                meshRenderer = GetComponent<MeshRenderer>();
            }
            else if (objectType == ObjectType.SkinnedMesh)
                skr = GetComponent<SkinnedMeshRenderer>();
            
            rest = GetComponent<RayfireRestriction>();

            // Add missing mesh renderer
            if (meshFilter != null && meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            // Init reset lists
            if (reset.action == RFReset.PostDemolitionType.DeactivateToReset)
                limitations.desc = new List<RayfireRigid>();
        }
        
        // Define components
        public void SetComponentsPhysics()
        {
            // Excluded from simulation
            if (physics.exclude == true)
                return;
            
            // Physics components
            physics.rigidBody = GetComponent<Rigidbody>();
            physics.meshCollider = GetComponent<Collider>();
            
            // Mesh Set collider
            if (objectType == ObjectType.Mesh)
                RFPhysic.SetRigidCollider (this);
            
            // Cluster check
            if (objectType == ObjectType.NestedCluster || objectType == ObjectType.ConnectedCluster) 
                RFDemolitionCluster.Clusterize (this);
            
            // Rigid body
            if (Application.isPlaying == true)
                if (simulationType != SimType.Static)
                    if (physics.rigidBody == null)
                        physics.rigidBody = gameObject.AddComponent<Rigidbody>();
        }

        /// /////////////////////////////////////////////////////////
        /// MeshRoot
        /// /////////////////////////////////////////////////////////

        // Setup mesh root editor ops
        void EditorSetupMeshRoot()
        {
            // Check if manager should be destroyed after setup
            bool destroyMan = RayfireMan.inst == null;

            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();
            
            // Reset
            ResetMeshRootSetup();
                
            // Setup
            SetupMeshRoot();
                
            // Destroy manager
            if (destroyMan == true && RayfireMan.inst != null)
                DestroyImmediate (RayfireMan.inst.transform.gameObject);
        }
        
        // Init mesh root. Copy Rigid component for children with mesh
        bool SetupMeshRoot()
        {
            if (objectType == ObjectType.MeshRoot)
            {
                // Get transform for Editor setup
                //if (transForm == null)
                //    transForm = GetComponent<Transform>();
                
                // Stop if already initiated
                if (limitations.demolished == true || physics.exclude == true)
                    return true;
                
                // Save tm
                physics.SaveInitTransform (transform);

                // MeshRoot Integrity check
                if (Application.isPlaying == true)
                    RFLimitations.MeshRootCheck(this);

                // Add Rigid to mesh Root children
                if (HasFragments == false)
                    AddMeshRootRigid(transform);
                
                // Init in runtime. DO not if editor setup
                if (Application.isPlaying == true)
                {
                    for (int i = 0; i < fragments.Count; i++)
                    {
                        fragments[i].Initialize();
                        fragments[i].meshRoot = this;
                    }
                }

                // Editor only ops
                if (Application.isPlaying == false)
                {
                    for (int i = 0; i < fragments.Count; i++)
                    {
                        // Set basic fragments components for collider apply
                        fragments[i].SetComponentsBasic();

                        // Set bound and size for connection size by bounding box
                        RFLimitations.SetBound (fragments[i]);
                    }
                    
                    // Add colliders to speedup. Editor only. Frags get collider at runtime in Initialize()
                    RFPhysic.SetupMeshRootColliders (this);
                }
                
                // Ignore neib collisions
                RFPhysic.SetIgnoreColliders (physics, fragments);
                
                // Runtime only ops
                if (Application.isPlaying == true)
                {
                    // Copy components. 
                    RayfireShatter.CopyRootMeshShatter (this, fragments);
                    RFPoolingParticles.CopyParticlesMeshroot (this, fragments);
                    
                    // Copy sound
                    sound = GetComponent<RayfireSound>();
                    RFSound.CopySound (sound, fragments);
                }
                
                // Set unyielding 
                RayfireUnyielding.MeshRootSetup (this);

                // Initialize connectivity
                InitConnectivity();
                
                // Turn off demolition and physics
                if (Application.isPlaying == true)
                {
                    demolitionType  = DemolitionType.None;
                    physics.exclude = true;
                    initialized     = true;
                }

                return true;
            }

            return false;
        }
        
        // Add Rigid to mesh Root children
        void AddMeshRootRigid(Transform tm)
        {
            // Get children
            List<Transform> children = new List<Transform>(tm.childCount);
            for (int i = 0; i < tm.childCount; i++)
                children.Add (tm.GetChild (i));
            
            // Add Rigid to child with mesh
            fragments = new List<RayfireRigid>();
            for (int i = 0; i < children.Count; i++)
            {
                MeshFilter mf = children[i].GetComponent<MeshFilter>();
                if (mf != null)
                {
                    // Get rigid
                    RayfireRigid childRigid = children[i].gameObject.GetComponent<RayfireRigid>();
                    
                    // Mark Rigid as custom Rigid component to keep it at Mesh Root Reset
                    if (childRigid != null)
                        childRigid.rootParent = tm;

                    // Add new and copy props from parent
                    if (childRigid == null)
                    {
                        childRigid = children[i].gameObject.AddComponent<RayfireRigid>();
                        CopyPropertiesTo (childRigid);
                        
                        // Copy Runtime caching properties. They are disabled for base copy
                        childRigid.meshDemolition.ch.CopyFrom (meshDemolition.ch);
                    }
                    
                    // Set meshfilter
                    childRigid.meshFilter = mf;

                    // Collect
                    fragments.Add (childRigid);

                    // Set parent meshRoot. IMPORTANT needed in case of custom Rigid
                    childRigid.meshRoot = this;
                }
            }
        }
        
        // Init connectivity if has
        void InitConnectivity()
        {
            activation.cnt = GetComponent<RayfireConnectivity>();
            if (activation.cnt != null && activation.cnt.rigidRootHost == null)
            {
                activation.cnt.meshRootHost = this;
                activation.cnt.Initialize();
            }
            
            // Warnings
            if (RayfireMan.debugStatic == true)
                if (activation.con == true && activation.cnt == null)
                    Debug.Log ("RayFireRigid: " + name + " object has enabled Connectivity activation but has no Connectivity component.", gameObject);
        }
        
        // Reset MeshRoot Setup
        void ResetMeshRootSetup()
        {
            // Reset Connectivity
            if (activation.cnt != null)
                activation.cnt.ResetSetup();
            activation.cnt = null;
            
            // ReSet unyielding 
            RayfireUnyielding.ResetMeshRootSetup (this);
            
            // Destroy new Rigid and clear custom Rigid components
            if (HasFragments == true)
            {
                if (physics.clusterColliders != null)
                {
                    // Clean fragments
                    for (int i = fragments.Count - 1; i >= 0; i--)
                        if (fragments[i] == null)
                            fragments.RemoveAt (i);

                    // Destroy colliders added by setup
                    HashSet<Collider> collidersHash = new HashSet<Collider> (physics.clusterColliders);
                    for (int i = 0; i < fragments.Count; i++)
                        if (fragments[i].physics.meshCollider != null)
                            if (collidersHash.Contains (fragments[i].physics.meshCollider) == false)
                                DestroyImmediate (fragments[i].physics.meshCollider);
                    physics.clusterColliders = null;

                    // Destroy Rigids added by setup
                    for (int i = 0; i < fragments.Count; i++)
                        if (fragments[i].rootParent == null)
                            DestroyImmediate (fragments[i]);
                        else
                        {
                            fragments[i].rootParent           = null;
                            fragments[i].meshFilter           = null;
                            fragments[i].meshRenderer         = null;
                            fragments[i].physics.meshCollider = null;
                            fragments[i].meshRoot             = null;
                        }
                }
            }

            // Reset common
            transForm          = null;
            physics.ignoreList = null;
            fragments          = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Start ops
        /// /////////////////////////////////////////////////////////
        
        // Set Start variables
        public void SetObjectType ()
        {
            if (objectType == ObjectType.Mesh ||
                objectType == ObjectType.NestedCluster ||
                objectType == ObjectType.ConnectedCluster)
            
                // Reset rigid data
                Default();
                
                // Set physics properties
                SetPhysics();
        }
        
        // Reset rigid data
        public void Default()
        {
            // Reset
            limitations.LocalReset();
            meshDemolition.LocalReset();
            clusterDemolition.LocalReset();
            
            limitations.birthTime = Time.time + Random.Range (0f, 0.05f);
           
            // Birth position for activation check
            physics.SaveInitTransform (transForm);

            // Set bound and size
            RFLimitations.SetBound(this);

            // Backup original layer
            RFActivation.BackupActivationLayer (this);

            // meshDemolition.properties.layerBack = gameObject.layer;
            // gameObject.tag;
        }
        
        // Set physics properties
        void SetPhysics()
        {
            // Excluded from sim
            if (physics.exclude == true)
                return;

            // MeshCollider physic material preset. Set new or take from parent 
            RFPhysic.SetColliderMaterial (this);

            // Set debris collider material
            // if (HasDebris == true) RFPhysic.SetParticleColliderMaterial (debrisList);
            
            // Ops with rigidbody applied
            if (physics.rigidBody != null)
            {
                // Set physical simulation type. Important. Should after collider material define
                if (Application.isPlaying == true)
                    RFPhysic.SetSimulationType (physics.rigidBody, simulationType, objectType, physics.gr, physics.si, physics.st);

                // Do not set convex, mass, drag for static
                if (simulationType == SimType.Static)
                    return;
                
                // Convex collider meshCollider. After SetSimulation Type to turn off convex for kinematic
                RFPhysic.SetColliderConvex (this);

                // Set density. After collider defined
                RFPhysic.SetDensity (this);

                // Set drag properties
                RFPhysic.SetDrag (this);
            }

            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////
        
        // Start all coroutines
        public void StartAllCoroutines()
        {
            // Stop if static
            if (simulationType == SimType.Static)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Check for demolition state every frame
            if (demolitionType != DemolitionType.None)
                StartCoroutine (limitations.DemolishableCor(this));
            
            // Offset fade
            if (fading.byOffset > 0)
            {
                fading.offsetEnum = RFFade.FadeOffsetCor (this);
                StartCoroutine (fading.offsetEnum);
            }

            // Start inactive coroutines
            InactiveCors();
            
            // Cache physics data for fragments 
            physics.physicsEnum = physics.PhysicsDataCor (this);
            StartCoroutine (physics.physicsEnum);

            // All coroutines are running
            corState = true;
        }

        // Start inactive coroutines
        public void InactiveCors()
        {
            // Activation by velocity\offset coroutines
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
            {
                if (activation.vel > 0)
                {
                    activation.velocityEnum = activation.ActivationVelocityCor (this);
                    StartCoroutine (activation.velocityEnum);
                }

                if (activation.off > 0)
                {
                    activation.offsetEnum = activation.ActivationOffsetCor (this);
                    StartCoroutine (activation.offsetEnum);
                }
            }

            // Init inactive every frame update coroutine
            if (simulationType == SimType.Inactive)
                //RayfireMan.inst.AddInactive (this);
                StartCoroutine (activation.InactiveCor(this));
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition types
        /// /////////////////////////////////////////////////////////
        
        // Awake Mesh input // TODO add checks in case has input mesh but mesh input is off
        public void MeshInput()
        {
            if (objectType == ObjectType.Mesh && 
                demolitionType == DemolitionType.Runtime && 
                meshDemolition.inp == RFDemolitionMesh.MeshInputType.AtStart)
            {
                // Set components for mesh / skinned mesh / clusters
                SetComponentsBasic();

                // Input
                RFFragment.InputMesh (this);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////

        // Collision check
        protected virtual void OnCollisionEnter (Collision collision)
        {
            // No demolition allowed
            if (demolitionType == DemolitionType.None)
                return;
            
            // Check if collision data needed
            if (limitations.CollisionCheck(this) == false)
                return;

            // Demolish object check
            if (DemolitionState() == false) 
                return;

            // Tag check. IMPORTANT keep length check for compatibility with older builds
            if (limitations.tag.Length > 0 && limitations.tag != "Untagged" && collision.collider.CompareTag (limitations.tag) == false)
                return;
            
            // Check if collision demolition passed
            if (CollisionDemolition (collision) == true)
                limitations.demolitionShould = true;
        }
        
        // Check if collision demolition passed
        protected virtual bool CollisionDemolition (Collision collision)
        {
            // Final object solidity
            float finalSolidity = physics.solidity * limitations.sol * RayfireMan.inst.globalSolidity;

            // Demolition by collision
            if (limitations.col == true)
            {
                // Collision with kinematic object. Uses collision.impulse
                if (limitations.KinematicCollisionCheck(collision, finalSolidity) == true)
                    return true;

                // Collision force checks. Uses relativeVelocity
                if (limitations.ContactPointsCheck(collision, finalSolidity) == true)
                    return true;
            }

            // Demolition by accumulated damage collision
            if (damage.en == true && damage.col == true)
                if (limitations.DamagePointsCheck(collision, this) == true)
                    return true;

            return false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Demolition
        /// /////////////////////////////////////////////////////////

        // Demolition available state
        public bool State ()
        {
            // Object already demolished
            if (limitations.demolished == true)
                return false;
           
            // Object already passed demolition state and demolishing is in progress
            if (meshDemolition.ch.inProgress == true)
                return false;
            
            // Bad mesh check
            if (meshDemolition.badMesh > RayfireMan.inst.advancedDemolitionProperties.badMeshTry)
                return false;
            
            // Max amount check
            if (RayfireMan.MaxAmountCheck == false)
                return false;
            
            // Depth level check
            if (limitations.depth > 0 && limitations.currentDepth >= limitations.depth)
                return false;
            
            // Min Size check. Min Size should be considered and size is less than
            if (limitations.bboxSize < limitations.size)
                return false;

            // Safe frame
            if (Time.time - limitations.birthTime < limitations.time)
                return false;
           
            // Static type objects can not be demolished
            if (simulationType == SimType.Static)
                return false;
            
            // Static objects can not be demolished
            if (gameObject.isStatic == true)
                return false;
            
            // Fading
            if (fading.state == 2)
                return false;
            
            return true;
        }
        
        // Check if object should be demolished
        public virtual bool DemolitionState ()
        {
            // No demolition allowed
            if (demolitionType == DemolitionType.None)
                return false;
            
            // Non destructible material
            if (physics.destructible == false)
                return false;
            
            // Visibility check
            if (Visible == false)
                return false;
            
            // Demolition available check
            if (State() == false)
                return false;
            
            // Per frame time check
            if (RayfireMan.inst.timeQuota > 0 && RayfireMan.inst.maxTimeThisFrame > RayfireMan.inst.timeQuota)
                return false;
            
            return true;
        }
        
        // Demolish object
        public void Demolish()
        {
            // Initialize if not
            if (initialized == false)
            {
                Initialize();
            }

            // Timestamp
            float t1 = Time.realtimeSinceStartup;

            // Demolish mesh or cluster to reference
            if (RFReferenceDemolition.DemolishReference(this) == false)
                return;

            // Demolish mesh and create fragments. Stop if runtime caching or no meshes/fragments were created
            if (RFDemolitionMesh.DemolishMesh (this) == true)
            {
                // Check for inactive/kinematic fragments with unyielding
                RayfireUnyielding.SetUnyieldingFragments (this);

                // Set children with mesh as additional fragments
                RFDemolitionMesh.ChildrenToFragments(this);
                
                // Clusterize runtime fragments
                RFDemolitionMesh.ClusterizeFragments (this);
            }
            else
                return;
            
            // Demolish cluster to children nodes 
            if (RFDemolitionCluster.DemolishCluster (this) == true)
                return;

            // Check fragments and proceed TODO separate flow for connected cls demolition
            if (limitations.demolished == false)
            {
                limitations.demolitionShould = false;
                demolitionType = DemolitionType.None;
                return;
            }
            
            // Connectivity check
            activation.CheckConnectivity();
            
            // Fragments initialisation
            InitMeshFragments();
            
            // Sum total demolition time
            RayfireMan.inst.maxTimeThisFrame += Time.realtimeSinceStartup - t1;
            
            // Init particles
            RFPoolingEmitter.SetHostDemolition(this);

            // Init sound
            RFSound.DemolitionSound(sound, limitations.bboxSize);

            // Event
            RFDemolitionEvent.RigidDemolitionEvent (this);
            
            // Destroy demolished object
            RayfireMan.DestroyFragment (this, rootParent);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Fragments
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid properties from parent to fragments
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Set local meshRoot
            if (objectType == ObjectType.MeshRoot)
                toScr.meshRoot = this;
            else if (meshRoot != null)
                    toScr.meshRoot = meshRoot;

            // Object type
            toScr.objectType = objectType;
            if (objectType == ObjectType.MeshRoot || objectType == ObjectType.SkinnedMesh)
                toScr.objectType = ObjectType.Mesh;
            
            // Sim type
            toScr.simulationType = simulationType;
            if (objectType != ObjectType.MeshRoot)
                if (simulationType == SimType.Kinematic || simulationType == SimType.Static || simulationType == SimType.Sleeping)
                    toScr.simulationType = SimType.Dynamic;

            // Demolition type
            toScr.demolitionType = demolitionType;
            if (objectType != ObjectType.MeshRoot)
                if (demolitionType != DemolitionType.None)
                    toScr.demolitionType = DemolitionType.Runtime;

            // Copy physics
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            toScr.limitations.CopyFrom (limitations);
            toScr.meshDemolition.CopyFrom (meshDemolition);
            toScr.clusterDemolition.CopyFrom (clusterDemolition);

            // Copy reference demolition props
            if (objectType == ObjectType.MeshRoot)
                toScr.referenceDemolition.CopyFrom (referenceDemolition);
            
            toScr.materials.CopyFrom (materials);
            toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            toScr.reset.CopyFrom (reset, objectType);
        }
        
        // Fragments initialisation
        public void InitMeshFragments()
        {
            // No fragments
            if (HasFragments == false)
                return;
            
            // Set velocity
            RFPhysic.SetFragmentsVelocity (this);
            
            // Sum total new fragments amount
            RayfireMan.inst.advancedDemolitionProperties.ChangeCurrentAmount (fragments.Count);
            
            // Set ancestor and descendants 
            RFLimitations.SetAncestor (this);
            RFLimitations.SetDescendants (this);

            // Fading. move to fragment
            if (fading.onDemolition == true)
                fading.DemolitionFade (fragments);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Manual methods
        /// /////////////////////////////////////////////////////////
        
        // Clear cache info
        public void DeleteCache()
        {
            meshes   = null;
            pivots   = null;
            rfMeshes = null;
            subIds   = null;
        }
        
        // Delete fragments
        public void DeleteFragments()
        {
            // Destroy root
            if (rootChild != null)
            {
                if (Application.isPlaying == true)
                    Destroy (rootChild.gameObject);
                else
                    DestroyImmediate (rootChild.gameObject);

                // Clear ref
                rootChild = null;
            }

            // Clear array
            fragments = null;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Blade
        /// /////////////////////////////////////////////////////////

        // Add new slice plane
        public void AddSlicePlane (Vector3[] slicePlane)
        {
            // Not even amount of slice data
            if (slicePlane.Length % 2 == 1)
                return;

            // Add slice plane data
            if (limitations.slicePlanes == null)
                limitations.slicePlanes = new List<Vector3>();
            limitations.slicePlanes.AddRange (slicePlane);
        }
        
        // Slice object
        public void Slice()
        {
            // Check for slices
            if (HasSlices == false)
            {
                if (RayfireMan.debugStatic == true)
                    Debug.Log ("RayFire Rigid: " + name + " has no defined slicing planes.", gameObject);
                return;
            }
            
            // Slice
            if (IsMesh == true)
            {
                // Slice. Stop if failed
                if (RFDemolitionMesh.SliceMesh (this) == false)
                    return;
                
                // Set children with mesh as additional fragments
                RFDemolitionMesh.ChildrenToFragments(this);
            }
            else if (objectType == ObjectType.ConnectedCluster)
                RFDemolitionCluster.SliceConnectedCluster (this);

            // Particles
            RFPoolingEmitter.SetHostDemolition(this);

            // Sound
            RFSound.DemolitionSound(sound, limitations.bboxSize);
            
            // Event
            RFDemolitionEvent.RigidDemolitionEvent (this);

            // Destroy original
            if (IsMesh == true)
                RayfireMan.DestroyFragment (this, rootParent);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Caching
        /// /////////////////////////////////////////////////////////
        
        // Caching into meshes over several frames
        public void CacheFrames()
        {
            StartCoroutine (meshDemolition.ch.RuntimeCachingCor(this));
        }

        /// /////////////////////////////////////////////////////////
        /// Public methods
        /// /////////////////////////////////////////////////////////

        // Save init transform. Birth tm for activation check and reset
        [ContextMenu("SaveInitTransform")]
        public void SaveInitTransform ()
        {
            // Rigid save tm
            if (objectType == ObjectType.Mesh)
                physics.SaveInitTransform (transForm);
            
            // Mesh Root save tm
            else if (objectType == ObjectType.MeshRoot)
            {
                if (HasFragments == true)
                {
                    // Save for Rigids
                    for (int i = 0; i < fragments.Count; i++)
                        if (fragments[i] != null)
                            fragments[i].physics.SaveInitTransform (fragments[i].transForm);

                    // Save is connectivity backup cluster
                    if (activation.cnt != null && reset.connectivity == true )
                        if (activation.cnt.backup != null)
                            RFBackupCluster.SaveTmRecursive (activation.cnt.backup.cluster);
                }
            }
        }
        
        // Apply damage
        public bool ApplyDamage (float damageValue, Vector3 damagePoint, float damageRadius = 0f, Collider coll = null)
        {
            return RFDamage.ApplyDamage (this, damageValue, damagePoint, damageRadius, coll);
        }
        
        // Activate inactive object
        public void Activate(bool connCheck = true)
        {
            if (objectType != ObjectType.MeshRoot)
                RFActivation.ActivateRigid (this, connCheck);
            else
                for (int i = 0; i < fragments.Count; i++)
                    RFActivation.ActivateRigid (fragments[i], connCheck);
        }
        
        // Fade this object
        public void Fade()
        {
            if (objectType != ObjectType.MeshRoot)
                RFFade.FadeRigid (this);
            else
                for (int i = 0; i < fragments.Count; i++)
                    RFFade.FadeRigid (fragments[i]);
        }
        
        // Reset object
        public void ResetRigid()
        {
            RFReset.ResetRigid (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Other
        /// /////////////////////////////////////////////////////////
        
        // Destroy
        public void DestroyObject(GameObject go) { Destroy (go); }
        public void DestroyRigid(RayfireRigid rigid) { Destroy (rigid); }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        // Fragments/Meshes check
        public bool HasFragments { get { return fragments != null && fragments.Count > 0; } }
        public bool HasMeshes    { get { return meshes != null && meshes.Length > 0; } }
        public bool HasRfMeshes  { get { return rfMeshes != null && rfMeshes.Length > 0; } }
        public bool HasDebris    { get { return debrisList != null && debrisList.Count > 0; } }
        public bool HasDust      { get { return dustList != null && dustList.Count > 0; } }
        bool        HasSlices    { get { return limitations.slicePlanes != null && limitations.slicePlanes.Count > 0; } }
        public bool IsCluster    { get { return objectType == ObjectType.ConnectedCluster || objectType == ObjectType.NestedCluster; } }
        bool        IsMesh       { get { return objectType == ObjectType.Mesh || objectType == ObjectType.SkinnedMesh; } }
        
        // Check if object visible  // TODO add cluster visibility support
        bool Visible
        { get {
                if (objectType == ObjectType.Mesh && meshRenderer != null) return meshRenderer.isVisible;
                if (objectType == ObjectType.SkinnedMesh && skr != null) return skr.isVisible;
                return true;
        }}

        // CLuster Integrity
        public float AmountIntegrity
        {
            get
            {
                if (objectType == ObjectType.ConnectedCluster)
                    return  clusterDemolition.cluster.shards.Count * 100f / clusterDemolition.am * 100f;
                return 0f;
            }
        }
    }
}


