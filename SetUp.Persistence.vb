Imports System.IO

Partial Public Class SetUp
    Private lastConfigurationPath As String = ""

    Private Function CaptureSetup() As SetupSaveData
        Return New SetupSaveData With {
            .SavedAtUtc = DateTime.UtcNow,
            .BoxName = txtBoxName.Text,
            .StartDelay = txbStart.Text, .PostSession = txbPostSession.Text,
            .ICIDuration = txbICI.Text, .ICIEnabled = chkICIEnabled.Checked,
            .ICIRetractInputs = chkICIRetractInputs.Checked, .ICIStimulus = ICIStimulusType(),
            .RandomPresentation = CheckBox1.Checked, .ComponentOrder = txbComponentOrder.Text,
            .CommandMap = ArduinoCommands.Snapshot(), .ComponentCount = vCC, .Components = SnapshotComponents()
        }
    End Function

    Private Sub ApplySetup(settings As SetupSaveData)
        RestoreComponents(settings.Components)
        vCC = CByte(settings.ComponentCount)
        txtBoxName.Text = settings.BoxName
        txbStart.Text = settings.StartDelay
        txbPostSession.Text = settings.PostSession
        txbICI.Text = settings.ICIDuration
        chkICIEnabled.Checked = settings.ICIEnabled
        chkICIRetractInputs.Checked = settings.ICIRetractInputs
        ApplyICIStimulusType(settings.ICIStimulus)
        CheckBox1.Enabled = vCC >= 2
        CheckBox1.Checked = settings.RandomPresentation
        RandomCPres = settings.RandomPresentation
        txbComponentOrder.Text = settings.ComponentOrder
        ArduinoCommands.Configure(settings.CommandMap)
        UpdateICIState()
        UpdateComponentOrderState()
        RefreshComponentSummaryTable()
    End Sub

    Private Function ConfigurationDirectory() As String
        If lastConfigurationPath <> "" Then
            Dim previousDirectory As String = Path.GetDirectoryName(lastConfigurationPath)
            If Directory.Exists(previousDirectory) Then Return previousDirectory
        End If
        If Directory.Exists("C:\SavedPrograms") Then Return "C:\SavedPrograms"
        Return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    End Function

    Private Async Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Using dialog As New SaveFileDialog With {
            .Title = "Save program setup — choose a name and folder",
            .Filter = "Setup files (*.json)|*.json", .DefaultExt = "json", .AddExtension = True,
            .OverwritePrompt = True, .CheckPathExists = True, .RestoreDirectory = True,
            .InitialDirectory = ConfigurationDirectory(),
            .FileName = If(lastConfigurationPath = "", "Setup.json", Path.GetFileNameWithoutExtension(lastConfigurationPath) & ".json")
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            Try
                CaptureSetup().Save(dialog.FileName)
                lastConfigurationPath = Path.GetFullPath(dialog.FileName)
            Catch ex As Exception
                MessageBox.Show(Me, "Could not save the setup:" & Environment.NewLine & ex.Message,
                    "Save setup", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End Try
            Try
                Dim queued = ConnectionStorage.QueueSetup(lastConfigurationPath)
                If queued Then Await ConnectionStorage.SynchronizeAsync()
                If IsDisposed Then Return
                MessageBox.Show(Me, "Program setup saved to:" & Environment.NewLine & lastConfigurationPath &
                    If(queued, Environment.NewLine & Environment.NewLine & ConnectionStorage.Status, ""),
                    "Save setup", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Catch ex As Exception
                MessageBox.Show(Me, "The setup was saved locally, but could not be queued for RTDB. Save it again to retry." &
                    Environment.NewLine & ex.Message, "Save setup", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End Try
        End Using
    End Sub

    Private Sub btnLoad_Click(sender As Object, e As EventArgs) Handles btnLoad.Click
        Using dialog As New OpenFileDialog With {
            .Title = "Load setup", .Filter = "Saved setups (*.json;*.xml;*.txt)|*.json;*.xml;*.txt|All files (*.*)|*.*",
            .InitialDirectory = ConfigurationDirectory(), .CheckFileExists = True,
            .Multiselect = False, .RestoreDirectory = True
        }
            If dialog.ShowDialog(Me) <> DialogResult.OK Then Return
            LoadSetupFile(dialog.FileName)
        End Using
    End Sub

    Private Sub LoadSetupFile(filePath As String)
        Dim previous As SetupSaveData = CaptureSetup()
        Dim previousTestsPerformed As Boolean = TestsPerformed
        Try
            Dim isStructured As Boolean
            Using reader As New StreamReader(filePath)
                While reader.Peek() >= 0 AndAlso Char.IsWhiteSpace(ChrW(reader.Peek()))
                    reader.Read()
                End While
                isStructured = reader.Peek() = AscW("<"c) OrElse reader.Peek() = AscW("{"c)
            End Using
            If isStructured Then
                'Read and validate the complete file before updating any controls.
                ApplySetup(SetupSaveData.Read(filePath))
            Else
                LoadLegacyConfiguration(filePath)
            End If
            TestsPerformed = False
            lastConfigurationPath = Path.GetFullPath(filePath)
            MessageBox.Show(Me, "Setup loaded from:" & Environment.NewLine & lastConfigurationPath &
                Environment.NewLine & Environment.NewLine & "Use Add box to add boxes with this configuration.",
                "Load setup", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Catch ex As Exception
            ApplySetup(previous)
            TestsPerformed = previousTestsPerformed
            MessageBox.Show(Me, "Could not load the setup. The previous configuration was restored." &
                Environment.NewLine & ex.Message, "Load setup", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class
