﻿/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections.Generic;
using Oculus.Interaction.PoseDetection.Debug;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection
{
    public class SequenceActiveState : MonoBehaviour, IActiveState
    {
        [Tooltip("The Sequence that will drive this component.")]
        [SerializeField]
        private Sequence _sequence;

        [Tooltip("If true, this ActiveState will become Active as soon " +
            "as the first sequence step becomes Active.")]
        [SerializeField]
        private bool _activateIfStepsStarted;

        [Tooltip("If true, this ActiveState will be active when " +
            "the supplied Sequence is Active.")]
        [SerializeField]
        private bool _activateIfStepsComplete = true;

        protected virtual void Start()
        {
            this.AssertField(_sequence, nameof(_sequence));
        }

        public bool Active
        {
            get
            {
                return (_activateIfStepsStarted && _sequence.CurrentActivationStep > 0 && !_sequence.Active) ||
                       (_activateIfStepsComplete && _sequence.Active);
            }
        }

        static SequenceActiveState()
        {
            ActiveStateDebugTree.RegisterModel<SequenceActiveState>(new DebugModel());
        }

        private class DebugModel : ActiveStateModel<SequenceActiveState>
        {
            protected override IEnumerable<IActiveState> GetChildren(SequenceActiveState activeState)
            {
                return new[] { activeState._sequence };
            }
        }

        #region Inject

        public void InjectAllSequenceActiveState(Sequence sequence,
            bool activateIfStepsStarted, bool activateIfStepsComplete)
        {
            InjectSequence(sequence);
            InjectActivateIfStepsStarted(activateIfStepsStarted);
            InjectActivateIfStepsComplete(activateIfStepsComplete);
        }

        public void InjectSequence(Sequence sequence)
        {
            _sequence = sequence;
        }

        public void InjectActivateIfStepsStarted(bool activateIfStepsStarted)
        {
            _activateIfStepsStarted = activateIfStepsStarted;
        }

        public void InjectActivateIfStepsComplete(bool activateIfStepsComplete)
        {
            _activateIfStepsComplete = activateIfStepsComplete;
        }

        #endregion
    }
}
