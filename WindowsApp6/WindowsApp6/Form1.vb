Imports System.IO
Imports System.Text
Imports System.Linq
Imports System.Tuple
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Numerics
Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click 'Enables us to click buttons
        If (FolderBrowserDialog1.ShowDialog() = DialogResult.OK) Then 'allows us to choose folder to open
            TextBox1.Text = FolderBrowserDialog1.SelectedPath 'Returns selected path to the textbox / Confirms and shows us that we have a functioning location assigned
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click 'Same as button 1 except for print out location rather than open location
        If (FolderBrowserDialog2.ShowDialog() = DialogResult.OK) Then 'also checks that a valid path is assigned
            TextBox2.Text = FolderBrowserDialog2.SelectedPath
            If String.IsNullOrEmpty(TextBox4.Text) Then
                TextBox4.Text = FolderBrowserDialog2.SelectedPath
            End If
        End If
    End Sub

    Public Function TextBoxassign()
        If TextBox1.Text <> Nothing And TextBox2.Text <> Nothing Then 'Checks that input and output files are assigned
            Dim newfile = "" 'creates empty string
            Dim ncount = 0 'sets numbercount for later use\ used to allow us to use same name for several files in neural network
            For Each foundFile As String In My.Computer.FileSystem.GetFiles(TextBox1.Text) 'starts a forloop in order to fourrier transform files as the are assigned
                Dim tempArray As Byte() = File.ReadAllBytes(foundFile).ToArray 'Reads our file
                Dim header = tempArray.Take(44) 'sepparately assigns header information
                Dim dataRaw = tempArray.Skip(44).ToArray 'skips header information enabling use of the raw data
                Dim inputlength As String = header(43).ToString("X") + header(42).ToString("X") + header(41).ToString("X") + header(40).ToString("X") 'gives us samples in the File
                Dim filesize As Integer = Convert.ToInt32(inputlength, 16) 'converts samples in file to Decimal
                Dim monostereo = header(23).ToString("X") + header(22).ToString("X") 'checks wether mono or stereo file
                Dim sjekk = header(27).ToString("X") + header(26).ToString("X") + header(25).ToString("X") + header(24).ToString("X") 'Gives us samples per second
                Dim sj = Convert.ToInt32(sjekk, 16)
                Dim vb = dataRaw.GetLength(0).ToString 'i think this was just to check an error i got because i read the file wrong
                Dim channels = Convert.ToInt32(monostereo, 16)
                Dim bpstest = header(34).ToString + header(33).ToString 'gives us bits per sample
                Dim bps = Convert.ToInt32(bpstest)
                Dim samples = (filesize / 2) 'gives us som definitions we will use later / remains from debugging
                Dim genvar As List(Of Integer)
                Dim genvarmax As Integer = 0
                If channels = 1 Then 'remains from debugging was initially planning on stereo file support but i have scrapped it as such only mono supported anyway
                    genvar = ConvertData(samples, dataRaw) 'gives us hex Raw data converted from binary to Decimal through hexadecimal and puts the raw data in the correct order
                    genvarmax = Converttohexlargestint(genvar, samples - 1) 'gives us largest sample used to convert to fourrier
                    Dim transformed(samples) As Double
                    Dim checkout = 0
                    For i = 0 To samples - 1
                        transformed(i) = Trnsfrm(genvar(i), genvarmax) 'divides every sample on the largest sample which enables fourriertransfromation
                        checkout = checkout + 1
                        If Trnsfrm(genvar(i), genvarmax) = 0 Then
                            transformed(i) = 0
                        End If
                    Next
                    Chart(samples - 1, transformed, "Filechart") 'charts what we have so far, only works when certain conditions are met currently not working
                    Dim deusvult = Fourriertransform(transformed, samples - 1, sj / 2) 'fourriertransforms our file, based on samples in file and the recording frequency
                    Chart(sj / 2, deusvult, "FileChart") 'charts fourrier transform
                    newfile = TextBox2.Text + "\" 'newfile is used to write our fourriertransformed file
                    Dim suffix = "0" 'suffix used to standardise nomenclature which makes reading into neural net easier
                    If ncount < 10 Then
                        newfile = newfile + TextBox8.Text + suffix + ncount.ToString + ".txt"
                    Else
                        newfile = newfile + TextBox8.Text + ncount.ToString + ".txt"
                    End If
                    Dim stringusdingus
                    For i = 0 To (sj / 2) - 1
                        stringusdingus = stringusdingus + vbTab + deusvult(i).ToString 'creates a long array with sepparators between each new value
                    Next
                    My.Computer.FileSystem.WriteAllText(newfile, stringusdingus, True) 'writes fourriertransformed file
                    ncount = ncount + 1
                ElseIf channels = 2 Then
                    MessageBox.Show("Stereo currently not supported, Mono files only please")
                End If
            Next
            MessageBox.Show("Operation Complete") 'shows a message when fourriertransformation is finished
        Else

            MessageBox.Show("You require both input and output path") 'incase you have not assigned input and output we will return an error message
        End If
    End Function

    Public Function ConvertData(Input2, dataRaw) '.Hex conversion
        Dim h5 = 0 'predefines a few values
        Dim h3 = 0
        Dim h4 As New List(Of Integer)()
        Dim h6 = 0

        For i = 0 To Input2 - 1
            h5 = Converttohex(dataRaw(i * 2), dataRaw(1 + i * 2)) 'puts dataRaw in correct order and converts from binary through hexadecimal to integer
            h4.Add(h5) 'appends new values to our list
        Next
        Return h4 ' returns list to main
    End Function

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        TextBoxassign() 'initiates the process of fourrier transfromation
    End Sub
    Function Converttohex(b1, b2)
        Dim h1 'predefines some values
        Dim h2
        Dim b3
        Dim b4 = 0
        Dim h6
        h2 = Hex(b1) 'takes binary input and converts it to hex and reverses order of appearance to comply with little endian
        h1 = Hex(b2)
        Dim TC = (2 ^ 16) / 2 'useful for fixing negative binary giving us underflow errors
        If Len(h1) = 1 Then
            h1 = "0" + h1 'when converted to hex from binary the computer tends to remove 0 if binary number is lower than decimal(16) we add it back
        End If
        If Len(h2) = 1 Then
            h2 = "0" + h2
        End If
        Dim h3 = h1 + h2 ' puts hexadecimal  values toghether
        h6 = Convert.ToInt32(h3, 16) ' converts said values to integer
        If h6 > TC Then
            h6 = h6 - (2 * TC) 'fixes binary to hexadecimal underflow
        End If

        Return h6 'returns h6 to convertdata
    End Function

    Public Function Converttohexlargestint(b, lenggu) 'gives us largest value
        Dim h4 = 0
        For i = 0 To lenggu
            Dim hc = Math.Abs(b(i)) 'goes  through list of the converted list of integers from the main fourrier sub
            If hc > h4 Then 'replaces hc with whatever the newest largest integer is
                h4 = hc
            End If
        Next

        Return h4 'returns largest value
    End Function

    Public Sub Chart(stringval, genvar, name) 'charting function
        Chart1.Series.Clear() 'clears chart which allows me to use a single chart series for all charting
        Chart1.Series.Add(name) 'gives us our chart series used in the below forloop
        For i = 0 To stringval
            Chart1.Series(name).ChartType = SeriesChartType.Column 'sets charttype
            Chart1.Series(name).Points.AddXY(i, genvar(i)) 'charts every XY point in whatever we are plotting
        Next
    End Sub

    Public Function Trnsfrm(bre, bri) 'divides sample from file on largest sample thus enabling fourriertransfromation
        Dim b As Double
        b = (bre / bri) 'all samples divided by largest sample
        Return b 'returns sample
    End Function
    Public Function Fourriertransform(Transformedgenvar, samples, wut) 'fourriertransform our file
        Dim fourrier(wut) As Double 'creates empty array of 22050 length(samples per second/2)
        For i = 0 To wut '0 to samples per second (22050)
            Dim genvarF As New Complex(0, 0) 'defines a complex cariable
            For j = 0 To samples 'total number of samples in our files 
                Dim c = ((2 * Math.PI / samples) * j * i) 'our complex variable for fourrier transfromation
                Dim cuck = Math.Cos(c) 'cosine part of our complex function
                Dim kek = -Math.Sin(c) 'Sine part of our complex funtion
                Dim kekvult As New Complex(cuck * Transformedgenvar(j), kek * Transformedgenvar(j)) 'uses complex function to fourrertransfrom our file
                genvarF = genvarF + kekvult 'creates long array of complex numbers
            Next
            If genvarF.Magnitude = 0 Then
                fourrier(i) = 0

            Else
                fourrier(i) = genvarF.Magnitude
            End If 'gives us magnitude of complex numbers, enabling our use of this as positive double
        Next
        Return fourrier 'returns our fourriertransformed variable to main fourrier sub
    End Function
    Public Function Transpose(matrix) 'transposes our matrix
        Dim xaksis = matrix.getLength(1) - 1 'defines x and y axis
        Dim yaksis = matrix.getlength(0) - 1
        Dim result(xaksis, yaksis)
        For i = 0 To xaksis
            For j = 0 To yaksis
                result(i, j) = matrix(j, i) 'transposes matrix itself
            Next

        Next
        Return result
    End Function
    Public Function Transposevector(matrix) 'transposes our matrix
        Dim xaksis = 0 'defines x and y axis
        Dim yaksis = matrix.getlength(0) - 1
        Dim result(xaksis, yaksis)
        For i = 0 To yaksis - 1
            For j = 0 To yaksis
                result(i, j) = matrix(j, i) 'transposes matrix itself
                xaksis = xaksis + 1
            Next

        Next
        Return result
    End Function
    Public Function Filecountcheck() ' am i still using this?
        Dim ncount = 0
        Dim checkname
        Dim filecount = IO.Directory.GetFiles(TextBox4.Text)
        For Each file As String In filecount
            Dim newfile = TextBox4.Text + file(ncount)
            If checkname.contains(Path.GetFileNameWithoutExtension(newfile)) <> True Then
                ncount = ncount + 1
            End If
            Return ncount
        Next

    End Function

    Public Function Sigmoid(x, deriv) 'defines our sigmoid function
        Dim storVAL(x.getlength(0) - 1, x.getlength(1) - 1)
        For i = 0 To x.getlength(0) - 1
            For j = 0 To x.getlength(1) - 1
                If deriv = True Then 'if i need the derivative then i can use it from calling the same function
                    storVAL(i, j) = (Math.Exp(x(i, j)) * (1 - Math.Exp(x(i, j))) ^ 2) 'derivative of my sigmoid
                End If

                storVAL(i, j) = 1 / (1 + Math.Exp(-x(i, j))) 'sigmoid, always returns number between 0 and 1
            Next
        Next
        Return storVAL
    End Function
    Public Function Sigmoidone(x, deriv) 'defines our sigmoid function
        Dim storVAL(x.getlength(0) - 1)
        For i = 0 To x.getlength(0) - 1
            If deriv = True Then 'if i need the derivative then i can use it from calling the same function
                storVAL(i) = (Math.Exp(x(i)) * (1 - Math.Exp(x(i))) ^ 2) 'derivative of my sigmoid
            End If

            storVAL(i) = 1 / (1 + Math.Exp(-x(i))) 'sigmoid, always returns number between 0 and 1

        Next
        Return storVAL
    End Function
    Public Function Randmatrix(x, y) 'provides matrix for our synapses
        MessageBox.Show(x)
        MessageBox.Show(y)
        Dim vectormatrix(x, y) As Double
        For i = 0 To x - 1
            For j = 0 To y - 1
                vectormatrix(i, j) = 1
            Next
        Next
        Return vectormatrix
    End Function

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        'Dim files = Filecountcheck()
        Dim temparray As String 'defines temporary array
        Dim output 'gives number of unique names
        Dim input = 22050 'spectrum on which fourriertransformation exists
        Dim names 'defines names of files
        Dim largestint = 0
        Dim names1
        Dim names2
        Dim names3
        Dim names4
        Dim names5
        Dim names6
        Dim names7
        Dim names8
        Dim names9
        Dim errorrate = 0
        Dim computerguess = Nothing
        Dim ans
        Dim correct = False
        Dim check = False
        Dim ncount = 0 'defines numbercount
        Dim syn0 = Synapse1(input) 'gives us synapses
        Dim newmatrix 'defines from new matrix
        Dim syn1 = synapse2(input, 10) 'gives us synapse 2
        Dim n2count = 0
        Dim alsocount = 0
        For Each foundFile As String In My.Computer.FileSystem.GetFiles(TextBox4.Text) 'gives us files from folder
            check = False
            Dim nibba = TextBox4.Text + foundFile 'gives us full extension of file and path
            Dim newFile = System.IO.Path.GetFileNameWithoutExtension(nibba) 'Gives us filename
            newFile = newFile.Remove(newFile.Length - 2) 'removes last characters from filename, which was added during fourrier transform
            If names <> newFile Then 'if we already took out one file with the same name, we can load its information
                If names = Nothing Then
                    names = newFile
                End If
                If names = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {1, 0, 0, 0, 0, 0, 0, 0, 0, 0}
                    ans = names
                    check = True
                    MessageBox.Show(ans)
                End If
            End If
            If names1 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names1 = Nothing Then
                    names1 = newFile
                End If
                If names1 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 1, 0, 0, 0, 0, 0, 0, 0, 0}
                    ans = names1
                    check = True
                End If

            End If
            If names2 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names2 = Nothing Then
                    names2 = newFile
                End If
                If names2 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 1, 0, 0, 0, 0, 0, 0, 0}
                    ans = names2
                    check = True
                End If
            End If
            If names3 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names3 = Nothing Then
                    names3 = newFile
                End If
                If names3 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 1, 0, 0, 0, 0, 0, 0}
                    ans = names3
                    check = True
                End If
            End If
            If names4 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names4 = Nothing Then
                    names4 = newFile
                End If
                If names4 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 1, 0, 0, 0, 0, 0}
                    ans = names4
                    check = True
                End If
            End If
            If names5 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names5 = Nothing Then
                    names5 = newFile
                End If
                If names5 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 0, 1, 0, 0, 0, 0}
                    ans = names5
                    check = True
                End If
            End If
            If names6 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names6 = Nothing Then
                    names6 = newFile
                End If
                If names6 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 0, 0, 1, 0, 0, 0}
                    ans = names6
                    check = True
                End If
            End If
            If names7 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names7 = Nothing Then
                    names7 = newFile
                End If
                If names7 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 0, 0, 0, 1, 0, 0}
                    ans = names7
                    check = True
                End If
            End If
            If names8 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names8 = Nothing Then
                    names8 = newFile
                End If
                If names8 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 0, 0, 0, 0, 1, 0}
                    ans = names8
                    check = True
                End If
            End If
            If names9 <> newFile And check <> True Then 'if we already took out one file with the same name, we can load its information
                If names9 = Nothing Then
                    names9 = newFile
                End If
                If names9 = newFile Then
                    temparray = File.ReadAllText(foundFile) 'reads out info from file
                    output = {0, 0, 0, 0, 0, 0, 0, 0, 0, 1}
                    ans = names9
                    check = True
                End If
            End If
            Dim words As String() = temparray.Split(New Char() {vbTab})
            Dim newwords(input) As Double
            Dim Trumphasthebest = words.Skip(1)
            Dim checkvalue = Trumphasthebest(3)
            For i = 0 To input - 1
                newwords(i) = Convert.ToDouble(Trumphasthebest(i))
            Next
            Dim l0 = newwords
            Dim Genvar15 = MatrixVectorMultiply(l0, syn0)
            Dim l1 = Sigmoid(Genvar15, False)
            Dim genvar9 = VectorMatrixMultiply(l1, syn1)
            Dim l2 = Sigmoidone(genvar9, False)
            Dim l2error = Errorcalc(output, l2)
            Dim l2delta = Deltacalc(l2error, Sigmoidone(l2, True))
            Dim l1error = Errorcalcmatrix(l2delta, Transpose(syn1))
            Dim l1delta = Deltacalcmatrix(l1error, Sigmoid(l1, True))
            syn1 = MatrixVectorMultiply(l2delta, Transpose(l1))
            syn0 = VectorMatrixMultiplyvectorend(l0, l1delta)
            For i = 0 To output.getlength - 1
                If l2(i) > largestint Then
                    l2(i) = largestint
                    If output(i) = 1 Then
                        correct = True
                        computerguess = ans
                    ElseIf output(7) = 1 And computerguess = Nothing Then
                        computerguess = names6
                    ElseIf output(1) = 1 And computerguess = Nothing Then
                        computerguess = names
                    ElseIf output(2) = 1 And computerguess = Nothing Then
                        computerguess = names1
                    ElseIf output(3) = 1 And computerguess = Nothing Then
                        computerguess = names2
                    ElseIf output(4) = 1 And computerguess = Nothing Then
                        computerguess = names3
                    ElseIf output(5) = 1 And computerguess = Nothing Then
                        computerguess = names4
                    ElseIf output(6) = 1 And computerguess = Nothing Then
                        computerguess = names5
                    ElseIf output(8) = 1 And computerguess = Nothing Then
                        computerguess = names7
                    ElseIf output(9) = 1 And computerguess = Nothing Then
                        computerguess = names8
                    ElseIf output(10) = 1 And computerguess = Nothing Then
                        computerguess = names9
                    End If
                End If
            Next
            n2count = n2count + 1
            If correct = True Then
                alsocount = alsocount + 1
            End If
            errorrate = alsocount / n2count
            TextBox10.Text = ans
            TextBox6.Text = computerguess
            Dim emptynumber = alsocount / n2count
            TextBox12.Text = errorrate
            Chart(9, l2, "Computerguess")
            MessageBox.Show(l2)
            MessageBox.Show(output)
            MessageBox.Show(ans)

        Next
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        If FolderBrowserDialog3.ShowDialog() = DialogResult.OK Then
            TextBox4.Text = FolderBrowserDialog3.SelectedPath
        End If
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs)
        If FolderBrowserDialog4.ShowDialog() = DialogResult.OK Then
            TextBox5.Text = FolderBrowserDialog4.SelectedPath
        End If
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs)
        If FolderBrowserDialog5.ShowDialog() = DialogResult.OK Then
            TextBox6.Text = FolderBrowserDialog5.SelectedPath
        End If
    End Sub
    Public Function Synapse1(input) 'gives us synapse 0 if we want to load synapses
        Dim syn0 = Randmatrix(input, (input / 50))
        Return syn0
    End Function
    Public Function synapse2(input, output) 'gives us synapse 0 if we want to load synapses
        Dim syn1 = Randmatrix((input / 50), 9)
        Return syn1
    End Function

    Public Function VectorMatrixMultiply(matrix1, matrix2)
        Dim matrix3(matrix2.getlength(1), matrix2.getlength(0)) As Double
        MessageBox.Show(matrix2.getlength(0))
        Dim Vectorans(10) As Double
        For i = 0 To matrix3.GetLength(0) - 2
            For j = 0 To matrix3.GetLength(1) - 2
                For k = 0 To matrix1.getlength(0) - 1
                    matrix3(i, j) = matrix1(k, j) * matrix2(j, i)
                    Vectorans(i) = Vectorans(i) + matrix3(i, j)

                Next
            Next
        Next
        Return Vectorans
    End Function
    Public Function VectorMatrixMultiplyvectorend(matrix1, matrix2)
        Dim matrix3(matrix2.getlength(1), matrix2.getlength(0)) As Double
        MessageBox.Show(matrix2.getlength(0))
        Dim Vectorans(22049, 440) As Double
        For i = 0 To matrix3.GetLength(0) - 2
            For j = 0 To matrix3.GetLength(1) - 2
                For k = 0 To matrix1.getlength(0) - 1
                    matrix3(i, j) = matrix1(k) * matrix2(j, i)
                Next
            Next
        Next
        Return Vectorans
    End Function
    Public Function MatrixVectorMultiply(matrix1, matrix2)
        Dim matrix3(matrix1.getlength(0) - 1, matrix2.getlength(1) - 1)
        For i = 0 To matrix3.GetLength(0) - 1
            For j = 0 To matrix3.GetLength(1) - 1
                matrix3(i, j) = matrix1(i) * matrix2(i, j)
            Next j
        Next i
        Return matrix3
    End Function
    Public Function Errorcalc(output, l2)
        Dim errcalc(output.getlength(0) - 1)
        For i = 0 To output.getlength(0) - 1
            errcalc(i) = output(i) - l2(i)
        Next
        Return errcalc
    End Function
    Public Function Errorcalcmatrix(output, l2)
        Dim err2calc(l2.getlength(0) - 1, l2.getlength(1) - 1)
        For i = 0 To l2.getlength(0) - 1
            For j = 0 To l2.getlength(1) - 1
                err2calc(i, j) = output(i) - l2(i, j)
            Next
        Next
        Return err2calc
    End Function
    Public Function Deltacalc(errcalc, sig)
        Dim Delta(errcalc.getlength(0) - 1)
        For i = 0 To errcalc.getlength(0) - 1
            Delta(i) = errcalc(i) * sig(i)
        Next
        Return Delta
    End Function
    Public Function Deltacalcmatrix(errcalc, sig)
        Dim Delta(sig.getlength(0) - 1, sig.getlength(1) - 1)
        For i = 0 To sig.getlength(0) - 1
            For j = 0 To sig.getlength(1) - 1
                For k = 0 To errcalc.getlength(0) - 1
                    Delta(i, j) = errcalc(k, j) * sig(i, j)
                Next
            Next
        Next
        Return Delta
    End Function
End Class
