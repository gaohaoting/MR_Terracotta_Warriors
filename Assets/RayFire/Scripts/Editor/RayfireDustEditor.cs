﻿using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine.Rendering;

namespace RayFire
{
    [CanEditMultipleObjects]
    [CustomEditor (typeof(RayfireDust))]
    public class RayfireDustEditor : Editor
    {
        RayfireDust        dust;
        List<string>       layerNames;
        SerializedProperty matListProp;
        ReorderableList    matList;

        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        const int space = 3;
        static bool exp_mat;
        static bool exp_emit;
        static bool exp_dyn;
        static bool exp_noise;
        static bool exp_coll;
        static bool exp_lim;
        static bool exp_rend;
        static bool exp_pool;
        
        static readonly GUIContent gui_emit_dml     = new GUIContent ("Demolition",     "");
        static readonly GUIContent gui_emit_act     = new GUIContent ("Activation",     "");
        static readonly GUIContent gui_emit_imp     = new GUIContent ("Impact",         "");
        static readonly GUIContent gui_main_op      = new GUIContent ("Opacity",        "");
        static readonly GUIContent gui_main_mat     = new GUIContent ("Material",       "");
        static readonly GUIContent gui_ems_tp       = new GUIContent ("Type",           "");
        static readonly GUIContent gui_ems_am       = new GUIContent ("Amount",         "");
        static readonly GUIContent gui_ems_var      = new GUIContent ("Variation",      "");
        static readonly GUIContent gui_ems_rate     = new GUIContent ("Rate",           "");
        static readonly GUIContent gui_ems_dur      = new GUIContent ("Duration",       "");
        static readonly GUIContent gui_ems_life_min = new GUIContent ("Life Min",       "");
        static readonly GUIContent gui_ems_life_max = new GUIContent ("Life Max",       "");
        static readonly GUIContent gui_ems_size_min = new GUIContent ("Size Min",       "");
        static readonly GUIContent gui_ems_size_max = new GUIContent ("Size Max",       "");
        static readonly GUIContent gui_ems_mat      = new GUIContent ("Material",       "");
        static readonly GUIContent gui_dn_speed_min = new GUIContent ("Speed Min",      "");
        static readonly GUIContent gui_dn_speed_max = new GUIContent ("Speed Max",      "");
        static readonly GUIContent gui_dn_grav_min  = new GUIContent ("Gravity Min",    "");
        static readonly GUIContent gui_dn_grav_max  = new GUIContent ("Gravity Max",    "");
        static readonly GUIContent gui_dn_rot       = new GUIContent ("Rotation",       "");
        static readonly GUIContent gui_ns_en        = new GUIContent ("Enable",         "");
        static readonly GUIContent gui_ns_qual      = new GUIContent ("Quality",        "");
        static readonly GUIContent gui_ns_str_min   = new GUIContent ("Strength Min",   "");
        static readonly GUIContent gui_ns_str_max   = new GUIContent ("Strength Max",   "");
        static readonly GUIContent gui_ns_freq      = new GUIContent ("Frequency",      "");
        static readonly GUIContent gui_ns_scroll    = new GUIContent ("Scroll Speed",   "");
        static readonly GUIContent gui_ns_damp      = new GUIContent ("Damping",        "");
        static readonly GUIContent gui_col_mask     = new GUIContent ("Collides With",  "");
        static readonly GUIContent gui_col_qual     = new GUIContent ("Quality",        "");
        static readonly GUIContent gui_col_rad      = new GUIContent ("Radius Scale",   "");
        static readonly GUIContent gui_lim_min      = new GUIContent ("Min Particles",  "");
        static readonly GUIContent gui_lim_max      = new GUIContent ("Max Particles",  "");
        static readonly GUIContent gui_lim_vis      = new GUIContent ("Visible",        "Emit dust if emitting object is visible in camera view");
        static readonly GUIContent gui_lim_perc     = new GUIContent ("Percentage",     "");
        static readonly GUIContent gui_lim_size     = new GUIContent ("Size Threshold", "");
        static readonly GUIContent gui_ren_cast     = new GUIContent ("Cast",           "");
        static readonly GUIContent gui_ren_rec      = new GUIContent ("Receive",        "");
        static readonly GUIContent gui_ren_prob     = new GUIContent ("Light Probes",   "");
        static readonly GUIContent gui_ren_vect     = new GUIContent ("Motion Vectors", "");
        static readonly GUIContent gui_ren_t        = new GUIContent ("Set Tag",        "");
        static readonly GUIContent gui_ren_tag      = new GUIContent ("Tag",            "");
        static readonly GUIContent gui_ren_l        = new GUIContent ("Set Layer",      "");
        static readonly GUIContent gui_ren_lay      = new GUIContent ("Layer",          "");
        static readonly GUIContent gui_pl_max       = new GUIContent ("Capacity",       "");
        static readonly GUIContent gui_pl_re        = new GUIContent ("Reuse",          "");
        static readonly GUIContent gui_pl_ov        = new GUIContent ("   Overflow",       "");
        static readonly GUIContent gui_pl_ph        = new GUIContent ("Warmup",         "Create all pool particles in Awake");
        static readonly GUIContent gui_pl_sk        = new GUIContent ("Skip",           "Do not instantiate debris particles if there are no any particles in the pool.");
        static readonly GUIContent gui_pl_rt        = new GUIContent ("Rate",           "Amount of particle systems that will be instantiated in pool every frame");
        static readonly GUIContent gui_pl_id        = new GUIContent ("Id",             "Emitter Pool Id");
        
        /// /////////////////////////////////////////////////////////
        /// Enable
        /// /////////////////////////////////////////////////////////
        
        private void OnEnable()
        {
            matListProp                 = serializedObject.FindProperty("dustMaterials");
            matList                     = new ReorderableList(serializedObject, matListProp, true, true, true, true)
            {
                drawElementCallback = DrawInitListItems,
                drawHeaderCallback  = DrawInitHeader,
                onAddCallback       = AddInit,
                onRemoveCallback    = RemoveInit
            };

            if (EditorPrefs.HasKey ("rf_um") == true) exp_mat   = EditorPrefs.GetBool ("rf_um");
            if (EditorPrefs.HasKey ("rf_ue") == true) exp_emit  = EditorPrefs.GetBool ("rf_ue");
            if (EditorPrefs.HasKey ("rf_ud") == true) exp_dyn   = EditorPrefs.GetBool ("rf_ud");
            if (EditorPrefs.HasKey ("rf_un") == true) exp_noise = EditorPrefs.GetBool ("rf_un");
            if (EditorPrefs.HasKey ("rf_uc") == true) exp_coll  = EditorPrefs.GetBool ("rf_uc");
            if (EditorPrefs.HasKey ("rf_ul") == true) exp_lim   = EditorPrefs.GetBool ("rf_ul");
            if (EditorPrefs.HasKey ("rf_ur") == true) exp_rend  = EditorPrefs.GetBool ("rf_ur");
            if (EditorPrefs.HasKey ("rf_up") == true) exp_pool  = EditorPrefs.GetBool ("rf_up");
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inspector
        /// /////////////////////////////////////////////////////////
        
        public override void OnInspectorGUI()
        {
            dust = target as RayfireDust;
            if (dust == null)
                return;
            
            GUILayout.Space (8);

            UI_Buttons();
            
            GUILayout.Space (space);
            
            UI_Emit();

            GUILayout.Space (space);

            UI_Main();

            GUILayout.Space (space);

            UI_Properties();
            
            GUILayout.Space (8);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Buttons
        /// /////////////////////////////////////////////////////////

        void UI_Buttons()
        {
            GUILayout.BeginHorizontal();

            if (Application.isPlaying == true)
            {
                if (GUILayout.Button ("Emit", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Emit();

                if (GUILayout.Button ("Clean", GUILayout.Height (25)))
                    foreach (var targ in targets)
                        if (targ as RayfireDust != null)
                            (targ as RayfireDust).Clean();
            }

            EditorGUILayout.EndHorizontal();
        }
        
        /// /////////////////////////////////////////////////////////
        /// Emit
        /// /////////////////////////////////////////////////////////
        
        void UI_Emit()
        {
            GUILayout.Label ("  Emit Event", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            bool onDemolition = EditorGUILayout.Toggle (gui_emit_dml, dust.onDemolition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects (targets, gui_emit_dml.text);
                foreach (RayfireDust scr in targets)
                {
                    scr.onDemolition = onDemolition;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            bool onActivation = EditorGUILayout.Toggle (gui_emit_act, dust.onActivation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects (targets, gui_emit_act.text);
                foreach (RayfireDust scr in targets)
                {
                    scr.onActivation = onActivation;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            bool onImpact = EditorGUILayout.Toggle (gui_emit_imp, dust.onImpact);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects (targets, gui_emit_imp.text);
                foreach (RayfireDust scr in targets)
                {
                    scr.onImpact = onImpact;
                    SetDirty (scr);
                }
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Main
        /// /////////////////////////////////////////////////////////

        void UI_Main()
        {
            GUILayout.Label ("  Dust", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            float opacity = EditorGUILayout.Slider (gui_main_op, dust.opacity, 0.01f, 1f);
            if (EditorGUI.EndChangeCheck() == true)
            {
                Undo.RecordObjects (targets, gui_main_op.text);
                foreach (RayfireDust scr in targets)
                {
                    scr.opacity = opacity;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);
            
            EditorGUI.BeginChangeCheck();
            Material dustMaterial = (Material)EditorGUILayout.ObjectField (gui_main_mat, dust.dustMaterial, typeof(Material), true);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects (targets, gui_main_mat.text);
                foreach (RayfireDust scr in targets)
                {
                    scr.dustMaterial = dustMaterial;
                    SetDirty (scr);
                }
            }

            GUILayout.Space (space);

            SetFoldoutPref (ref exp_mat, "rf_um", "Random Materials", true);
            if (exp_mat == true)
            {
                GUILayout.Space (space);

                serializedObject.Update();
                matList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Properties
        /// /////////////////////////////////////////////////////////
        
        void UI_Properties()
        {
            GUILayout.Label ("  Properties", EditorStyles.boldLabel);
            
            UI_Pool();
            
            GUILayout.Space (space);
            
            UI_Emission();

            GUILayout.Space (space);

            UI_Dynamic();

            GUILayout.Space (space);

            UI_Noise();

            GUILayout.Space (space);

            UI_Collision();

            GUILayout.Space (space);

            UI_Limitations();
            
            GUILayout.Space (space);

            UI_Rendering();
        }

        /// /////////////////////////////////////////////////////////
        /// Emission
        /// /////////////////////////////////////////////////////////
        
        void UI_Emission()
        {
            SetFoldoutPref (ref exp_emit, "rf_ue", "Emission", true);
            if (exp_emit == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Burst", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                RFParticles.BurstType burstType = (RFParticles.BurstType)EditorGUILayout.EnumPopup (gui_ems_tp, dust.emission.burstType);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_tp.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.burstType = burstType;
                        SetDirty (scr);
                    }
                }

                if (dust.emission.burstType != RFParticles.BurstType.None)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    int burstAmount = EditorGUILayout.IntSlider (gui_ems_am, dust.emission.burstAmount, 0, 500);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ems_am.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.emission.burstAmount = burstAmount;
                            SetDirty (scr);
                        }
                    }
                    
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    int burstVar = EditorGUILayout.IntSlider (gui_ems_var, dust.emission.burstVar, 0, 100);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ems_var.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.emission.burstVar = burstVar;
                            SetDirty (scr);
                        }
                    }
                }
                
                GUILayout.Space (space);

                GUILayout.Label ("      Distance", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float distanceRate = EditorGUILayout.Slider (gui_ems_rate, dust.emission.distanceRate, 0f, 5f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_rate.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.distanceRate = distanceRate;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                float duration = EditorGUILayout.Slider (gui_ems_dur, dust.emission.duration, 0.5f, 10);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_dur.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.duration = duration;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                GUILayout.Label ("      Lifetime", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float lifeMin = EditorGUILayout.Slider (gui_ems_life_min, dust.emission.lifeMin, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_life_min.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.lifeMin = lifeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                float lifeMax = EditorGUILayout.Slider (gui_ems_life_max, dust.emission.lifeMax, 1f, 60f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_life_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.lifeMax = lifeMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                GUILayout.Label ("      Size", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float sizeMin = EditorGUILayout.Slider (gui_ems_size_min, dust.emission.sizeMin, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_size_min.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.sizeMin = sizeMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                float sizeMax = EditorGUILayout.Slider (gui_ems_size_max, dust.emission.sizeMax, 0.1f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ems_size_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emission.sizeMax = sizeMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                GUILayout.Label ("      Material", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                Material emissionMaterial = (Material)EditorGUILayout.ObjectField (gui_ems_mat, dust.emissionMaterial, typeof(Material), true);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObjects (targets, gui_ems_mat.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.emissionMaterial = emissionMaterial;
                        SetDirty (scr);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Dynamic
        /// /////////////////////////////////////////////////////////
        
        void UI_Dynamic()
        {
            SetFoldoutPref (ref exp_dyn, "rf_ud", "Dynamic", true);
            if (exp_dyn == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Speed", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float speedMin = EditorGUILayout.Slider (gui_dn_speed_min, dust.dynamic.speedMin, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_dn_speed_min.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.speedMin = speedMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                float speedMax = EditorGUILayout.Slider (gui_dn_speed_max, dust.dynamic.speedMax, 0f, 10f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_dn_speed_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.speedMax = speedMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                GUILayout.Label ("      Gravity", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float gravityMin = EditorGUILayout.Slider (gui_dn_grav_min, dust.dynamic.gravityMin, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_dn_grav_min.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.gravityMin = gravityMin;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                float gravityMax = EditorGUILayout.Slider (gui_dn_grav_max, dust.dynamic.gravityMax, -2f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_dn_grav_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.gravityMax = gravityMax;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                GUILayout.Label ("      Rotation", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                float rotation = EditorGUILayout.Slider (gui_dn_rot, dust.dynamic.rotation, 0f, 1f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_dn_rot.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.dynamic.rotation = rotation;
                        SetDirty (scr);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Noise
        /// /////////////////////////////////////////////////////////
        
        void UI_Noise()
        {
            SetFoldoutPref (ref exp_noise, "rf_un", "Noise", true);
            if (exp_noise == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Main", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUILayout.Toggle (gui_ns_en, dust.noise.enabled);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ns_en.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.noise.enabled = enabled;
                        SetDirty (scr);
                    }
                }

                if (dust.noise.enabled == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    ParticleSystemNoiseQuality quality = (ParticleSystemNoiseQuality)EditorGUILayout.EnumPopup (gui_ns_qual, dust.noise.quality);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_qual.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.quality = quality;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    GUILayout.Label ("      Strength", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    float strengthMin = EditorGUILayout.Slider (gui_ns_str_min, dust.noise.strengthMin, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_str_min.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.strengthMin = strengthMin;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    float strengthMax = EditorGUILayout.Slider (gui_ns_str_max, dust.noise.strengthMax, 0f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_str_max.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.strengthMax = strengthMax;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);
                    
                    GUILayout.Label ("      Other", EditorStyles.boldLabel);

                    EditorGUI.BeginChangeCheck();
                    float frequency = EditorGUILayout.Slider (gui_ns_freq, dust.noise.frequency, 0.001f, 3f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_freq.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.frequency = frequency;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    float scrollSpeed = EditorGUILayout.Slider (gui_ns_scroll, dust.noise.scrollSpeed, 0f, 2f);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_scroll.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.scrollSpeed = scrollSpeed;
                            SetDirty (scr);
                        }
                    }

                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    bool damping = EditorGUILayout.Toggle (gui_ns_damp, dust.noise.damping);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_ns_damp.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.noise.damping = damping;
                            SetDirty (scr);
                        }
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Collision
        /// /////////////////////////////////////////////////////////
        
        void UI_Collision()
        {
            SetFoldoutPref (ref exp_coll, "rf_uc", "Collision", true);
            if (exp_coll == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Common", EditorStyles.boldLabel);
                
                // Layer mask
                if (layerNames == null)
                {
                    layerNames = new List<string>();
                    for (int i = 0; i <= 31; i++)
                        layerNames.Add (i + ". " + LayerMask.LayerToName (i));
                }

                EditorGUI.BeginChangeCheck();
                LayerMask collidesWith = EditorGUILayout.MaskField (gui_col_mask, dust.collision.collidesWith, layerNames.ToArray());
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_col_mask.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.collidesWith = collidesWith;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                ParticleSystemCollisionQuality quality = (ParticleSystemCollisionQuality)EditorGUILayout.EnumPopup (gui_col_qual, dust.collision.quality);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_col_qual.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.quality = quality;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                float radiusScale = EditorGUILayout.Slider (gui_col_rad, dust.collision.radiusScale, 0.1f, 2f);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_col_rad.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.collision.radiusScale = radiusScale;
                        SetDirty (scr);
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Limitations
        /// /////////////////////////////////////////////////////////

        void UI_Limitations()
        {
            SetFoldoutPref (ref exp_lim, "rf_ul", "Limitations", true);
            if (exp_lim == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Particle System", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                int minParticles = EditorGUILayout.IntSlider (gui_lim_min, dust.limitations.minParticles, 3, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_lim_min.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.minParticles = minParticles;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                int maxParticles = EditorGUILayout.IntSlider (gui_lim_max, dust.limitations.maxParticles, 5, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_lim_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.maxParticles = maxParticles;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bool visible = EditorGUILayout.Toggle (gui_lim_vis, dust.limitations.visible);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_lim_vis.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.visible = visible;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                GUILayout.Label ("      Fragments", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                int percentage = EditorGUILayout.IntSlider (gui_lim_perc, dust.limitations.percentage, 10, 100);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_lim_perc.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.percentage = percentage;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                float sizeThreshold = EditorGUILayout.Slider (gui_lim_size, dust.limitations.sizeThreshold, 0.05f, 5);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_lim_size.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.limitations.sizeThreshold = sizeThreshold;
                        SetDirty (scr);
                    }
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Rendering
        /// /////////////////////////////////////////////////////////

        void UI_Rendering()
        {
            SetFoldoutPref (ref exp_rend, "rf_ur", "Rendering", true);
            if (exp_rend == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                GUILayout.Label ("      Shadows", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();
                bool castShadows = EditorGUILayout.Toggle (gui_ren_cast, dust.rendering.castShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_cast.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.castShadows = castShadows;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bool receiveShadows = EditorGUILayout.Toggle (gui_ren_rec, dust.rendering.receiveShadows);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_rec.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.receiveShadows = receiveShadows;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                GUILayout.Label ("      Other", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                LightProbeUsage lightProbes = (LightProbeUsage)EditorGUILayout.EnumPopup (gui_ren_prob, dust.rendering.lightProbes);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_prob.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.lightProbes = lightProbes;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                MotionVectorGenerationMode motionVectors = (MotionVectorGenerationMode)EditorGUILayout.EnumPopup (gui_ren_vect, dust.rendering.motionVectors);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_vect.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.motionVectors = motionVectors;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bool t = EditorGUILayout.Toggle (gui_ren_t, dust.rendering.t);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_t.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.t = t;
                        SetDirty (scr);
                    }
                }

                if (dust.rendering.t == true)
                {
                    GUILayout.Space (space);
                    
                    EditorGUI.indentLevel++;
                    
                    EditorGUI.BeginChangeCheck();
                    string tag = EditorGUILayout.TagField (gui_ren_tag, dust.rendering.tag);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObjects (targets, gui_ren_tag.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.rendering.tag = tag;
                            SetDirty (scr);
                        }
                    }

                    EditorGUI.indentLevel--;
                }

                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bool l = EditorGUILayout.Toggle (gui_ren_l, dust.rendering.l);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ren_l.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.rendering.l = l;
                        SetDirty (scr);
                    }
                }
                
                if (dust.rendering.l == true)
                {
                    GUILayout.Space (space);
                    
                    EditorGUI.indentLevel++;
                    
                    EditorGUI.BeginChangeCheck();
                    int layer = EditorGUILayout.LayerField (gui_ren_lay, dust.rendering.layer);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObjects (targets, gui_ren_lay.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.rendering.layer = layer;
                            SetDirty (scr);
                        }
                    }

                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Pool
        /// /////////////////////////////////////////////////////////

        void UI_Pool()
        {
            SetFoldoutPref (ref exp_pool, "rf_up", "Pool", true);
            if (exp_pool == true)
            {
                GUILayout.Space (space);

                EditorGUI.indentLevel++;

                EditorGUI.BeginChangeCheck();
                int id = EditorGUILayout.IntSlider (gui_pl_id, dust.pool.id, 0, 99);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_id.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.id = id;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);
                
                EditorGUI.BeginChangeCheck();
                bool enable = EditorGUILayout.Toggle (gui_ns_en, dust.pool.enable);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_ns_en.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.Enable = enable;
                        SetDirty (scr);
                    }
                }
                
                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                bool warmup = EditorGUILayout.Toggle (gui_pl_ph, dust.pool.warmup);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_ph.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.warmup = warmup;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                int cap = EditorGUILayout.IntSlider (gui_pl_max, dust.pool.cap, 3, 300);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_max.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.Cap = cap;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                int rate = EditorGUILayout.IntSlider (gui_pl_rt, dust.pool.rate, 1, 15);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_rt.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.Rate = rate;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                bool skip = EditorGUILayout.Toggle (gui_pl_sk, dust.pool.skip);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_sk.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.Skip = skip;
                        SetDirty (scr);
                    }
                }

                GUILayout.Space (space);

                EditorGUI.BeginChangeCheck();
                bool reuse = EditorGUILayout.Toggle (gui_pl_re, dust.pool.reuse);
                if (EditorGUI.EndChangeCheck() == true)
                {
                    Undo.RecordObjects (targets, gui_pl_re.text);
                    foreach (RayfireDust scr in targets)
                    {
                        scr.pool.Reuse = reuse;
                        SetDirty (scr);
                    }
                }

                if (dust.pool.reuse == true)
                {
                    GUILayout.Space (space);

                    EditorGUI.BeginChangeCheck();
                    int over = EditorGUILayout.IntSlider (gui_pl_ov, dust.pool.over, 0, 10);
                    if (EditorGUI.EndChangeCheck() == true)
                    {
                        Undo.RecordObjects (targets, gui_pl_ov.text);
                        foreach (RayfireDust scr in targets)
                        {
                            scr.pool.Over = over;
                            SetDirty (scr);
                        }
                    }
                }
                
                // Caption
                if (dust.pool.enable == true && Application.isPlaying == true)
                {
                    GUILayout.Space (space);

                    if (dust.pool.emitter != null)
                        GUILayout.Label ("     Available : " + dust.pool.emitter.queue.Count, EditorStyles.boldLabel);
                }

                // Edit
                if (Application.isPlaying == true)
                {
                    GUILayout.Space (space);
                    if (GUILayout.Button ("Edit Emitter Particles", GUILayout.Height (20)))
                        foreach (var targ in targets)
                            if (targ as RayfireDust != null)
                                (targ as RayfireDust).EditEmitterParticles();
                }
                
                EditorGUI.indentLevel--;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// ReorderableList draw
        /// /////////////////////////////////////////////////////////
        
        void DrawInitListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = matList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y+2, EditorGUIUtility.currentViewWidth - 80f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
        }
        
        void DrawInitHeader(Rect rect)
        {
            rect.x += 10;
            EditorGUI.LabelField(rect, "Random Materials");
        }

        void AddInit(ReorderableList list)
        {
            if (dust.dustMaterials == null)
                dust.dustMaterials = new List<Material>();
            dust.dustMaterials.Add (null);
            list.index = list.count;
        }
        
        void RemoveInit(ReorderableList list)
        {
            if (dust.dustMaterials != null)
            {
                dust.dustMaterials.RemoveAt (list.index);
                list.index = list.index - 1;
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        void SetDirty (RayfireDust scr)
        {
            if (Application.isPlaying == false)
            {
                EditorUtility.SetDirty (scr);
                EditorSceneManager.MarkSceneDirty (scr.gameObject.scene);
            }
        }
        
        void SetFoldoutPref (ref bool val, string pref, string caption, bool state) 
        {
            EditorGUI.BeginChangeCheck();
            val = EditorGUILayout.Foldout (val, caption, state);
            if (EditorGUI.EndChangeCheck() == true)
                EditorPrefs.SetBool (pref, val);
        }
    }
}