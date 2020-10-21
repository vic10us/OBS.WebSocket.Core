/*
    TestClient for obs-websocket-dotnet
    Copyright (C) 2020 Keith Hill <stephane.lepin@gmail.com>

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program. If not, see <https://www.gnu.org/licenses/>
*/

using System;
using System.Windows.Forms;
using OBS.WebSockets.Core;
using OBS.WebSockets.Core.Types;

namespace TestClient
{
    public partial class MainWindow : Form
    {
        protected OBSWebsocket _obs;

        public MainWindow()
        {
            InitializeComponent();
            _obs = new OBSWebsocket {WSTimeout = TimeSpan.FromMinutes(10)};

            _obs.Connected += OnConnect;
            _obs.Disconnected += OnDisconnect;

            _obs.SceneChanged += OnSceneChange;
            _obs.SceneCollectionChanged += OnSceneColChange;
            _obs.ProfileChanged += OnProfileChange;
            _obs.TransitionChanged += OnTransitionChange;
            _obs.TransitionDurationChanged += OnTransitionDurationChange;

            _obs.StreamingStateChanged += OnStreamingStateChange;
            _obs.RecordingStateChanged += OnRecordingStateChange;

            _obs.StreamStatus += OnStreamData;
        }

        private void OnConnect(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => {
                txtServerIP.Enabled = false;
                txtServerPassword.Enabled = false;
                btnConnect.Text = "Disconnect";

                gbControls.Enabled = true;

                var versionInfo = _obs.GetVersion();
                tbPluginVersion.Text = versionInfo.PluginVersion;
                tbOBSVersion.Text = versionInfo.OBSStudioVersion;

                btnListScenes.PerformClick();
                btnGetCurrentScene.PerformClick();

                btnListSceneCol.PerformClick();
                btnGetCurrentSceneCol.PerformClick();

                btnListProfiles.PerformClick();
                btnGetCurrentProfile.PerformClick();

                btnListTransitions.PerformClick();
                btnGetCurrentTransition.PerformClick();

                btnGetTransitionDuration.PerformClick();

                var streamStatus = _obs.GetStreamingStatus();
                if (streamStatus.IsStreaming)
                    OnStreamingStateChange(_obs, OutputState.Started);
                else
                    OnStreamingStateChange(_obs, OutputState.Stopped);

                if (streamStatus.IsRecording)
                    OnRecordingStateChange(_obs, OutputState.Started);
                else
                    OnRecordingStateChange(_obs, OutputState.Stopped);
            }));
        }

        private void OnDisconnect(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)(() => {
                gbControls.Enabled = false;

                txtServerIP.Enabled = true;
                txtServerPassword.Enabled = true;
                btnConnect.Text = "Connect";
            }));
        }

        private void OnSceneChange(OBSWebsocket sender, string newSceneName)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbCurrentScene.Text = newSceneName;
            });
        }

        private void OnSceneColChange(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbSceneCol.Text = _obs.GetCurrentSceneCollection();
            });
        }

        private void OnProfileChange(object sender, EventArgs e)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbProfile.Text = _obs.GetCurrentProfile();
            });
        }

        private void OnTransitionChange(OBSWebsocket sender, string newTransitionName)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbTransition.Text = newTransitionName;
            });
        }

        private void OnTransitionDurationChange(OBSWebsocket sender, int newDuration)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                tbTransitionDuration.Value = newDuration;
            });
        }

        private void OnStreamingStateChange(OBSWebsocket sender, OutputState newState)
        {
            string state = "";
            switch(newState)
            {
                case OutputState.Starting:
                    state = "Stream starting...";
                    break;

                case OutputState.Started:
                    state = "Stop streaming";
                    BeginInvoke((MethodInvoker)delegate
                    {
                        gbStatus.Enabled = true;
                    });
                    break;

                case OutputState.Stopping:
                    state = "Stream stopping...";
                    break;

                case OutputState.Stopped:
                    state = "Start streaming";
                    BeginInvoke((MethodInvoker)delegate
                    {
                        gbStatus.Enabled = false;
                    });
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            BeginInvoke((MethodInvoker)delegate
            {
                btnToggleStreaming.Text = state;
            });
        }

        private void OnRecordingStateChange(OBSWebsocket sender, OutputState newState)
        {
            string state = "";
            switch (newState)
            {
                case OutputState.Starting:
                    state = "Recording starting...";
                    break;

                case OutputState.Started:
                    state = "Stop recording";
                    break;

                case OutputState.Stopping:
                    state = "Recording stopping...";
                    break;

                case OutputState.Stopped:
                    state = "Start recording";
                    break;

                default:
                    state = "State unknown";
                    break;
            }

            BeginInvoke((MethodInvoker)delegate
            {
                btnToggleRecording.Text = state;
            });
        }

        private void OnStreamData(OBSWebsocket sender, StreamStatus data)
        {
            BeginInvoke((MethodInvoker)delegate
            {
                // ReSharper disable LocalizableElement
                txtStreamTime.Text = $"{data.TotalStreamTime} sec";
                txtKbitsSec.Text = $"{data.KbitsPerSec} kb/s";
                txtBytesSec.Text = $"{data.BytesPerSec} bytes/s";
                txtFramerate.Text = $"{data.FPS} FPS";
                txtStrain.Text = $"{(data.Strain * 100)} %";
                txtDroppedFrames.Text = data.DroppedFrames.ToString();
                txtTotalFrames.Text = data.TotalFrames.ToString();
                // ReSharper restore LocalizableElement
            });
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if(!_obs.IsConnected)
            {
                try
                {
                    _obs.Connect(txtServerIP.Text, txtServerPassword.Text);
                }
                catch (AuthFailureException)
                {
                    MessageBox.Show("Authentication failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                catch (ErrorResponseException ex)
                {
                    MessageBox.Show("Connect failed : " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            } else
            {
                _obs.Disconnect();
            }
        }

        private void btnListScenes_Click(object sender, EventArgs e)
        {
            var scenes = _obs.ListScenes();

            tvScenes.Nodes.Clear();
            foreach(var scene in scenes)
            {
                var node = new TreeNode(scene.Name);
                foreach (var item in scene.Items)
                {
                    node.Nodes.Add(item.SourceName);
                }

                tvScenes.Nodes.Add(node);
            }
        }

        private void btnGetCurrentScene_Click(object sender, EventArgs e)
        {
            tbCurrentScene.Text = _obs.GetCurrentScene().Name;
        }

        private void btnSetCurrentScene_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentScene(tbCurrentScene.Text);
        }

        private void tvScenes_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbCurrentScene.Text = e.Node.Text;
            }
        }

        private void btnListSceneCol_Click(object sender, EventArgs e)
        {
            var sc = _obs.ListSceneCollections();

            tvSceneCols.Nodes.Clear();
            foreach (var sceneCol in sc)
            {
                tvSceneCols.Nodes.Add(sceneCol);
            }
        }

        private void btnGetCurrentSceneCol_Click(object sender, EventArgs e)
        {
            tbSceneCol.Text = _obs.GetCurrentSceneCollection();
        }

        private void btnSetCurrentSceneCol_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentSceneCollection(tbSceneCol.Text);
        }

        private void tvSceneCols_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbSceneCol.Text = e.Node.Text;
            }
        }

        private void btnListProfiles_Click(object sender, EventArgs e)
        {
            var profiles = _obs.ListProfiles();

            tvProfiles.Nodes.Clear();
            foreach (var profile in profiles)
            {
                tvProfiles.Nodes.Add(profile);
            }
        }

        private void btnGetCurrentProfile_Click(object sender, EventArgs e)
        {
            tbProfile.Text = _obs.GetCurrentProfile();
        }

        private void btnSetCurrentProfile_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentProfile(tbProfile.Text);
        }

        private void tvProfiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbProfile.Text = e.Node.Text;
            }
        }

        private void btnToggleStreaming_Click(object sender, EventArgs e)
        {
            _obs.ToggleStreaming();
        }

        private void btnToggleRecording_Click(object sender, EventArgs e)
        {
            _obs.ToggleRecording();
        }

        private void btnListTransitions_Click(object sender, EventArgs e)
        {
            var transitions = _obs.ListTransitions();

            tvTransitions.Nodes.Clear();
            foreach (var transition in transitions)
            {
                tvTransitions.Nodes.Add(transition);
            }
        }

        private void btnGetCurrentTransition_Click(object sender, EventArgs e)
        {
            tbTransition.Text = _obs.GetCurrentTransition().Name;
        }

        private void btnSetCurrentTransition_Click(object sender, EventArgs e)
        {
            _obs.SetCurrentTransition(tbTransition.Text);
        }

        private void tvTransitions_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                tbTransition.Text = e.Node.Text;
            }
        }

        private void btnGetTransitionDuration_Click(object sender, EventArgs e)
        {
            tbTransitionDuration.Value = _obs.GetCurrentTransition().Duration;
        }

        private void btnSetTransitionDuration_Click(object sender, EventArgs e)
        {
            _obs.SetTransitionDuration((int)tbTransitionDuration.Value);
        }

        private void btnAdvanced_Click(object sender, EventArgs e)
        {
            AdvancedWindow advanced = new AdvancedWindow();
            advanced.SetOBS(_obs);
            advanced.ShowDialog();
        }
    }
}