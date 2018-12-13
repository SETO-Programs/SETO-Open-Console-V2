Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Newtonsoft.Json
Imports System.Windows.Forms
Imports RapidAPISDK
Imports System.Net.NetworkInformation
Imports System.Net
Imports System.Text.RegularExpressions
Imports unirest_net.http
Imports System.Management
Imports System.Drawing
Imports System.Windows.Xps.Packaging
Imports System.Windows.Documents
Imports System.Windows.Media.Imaging
Imports PdfSharp
Imports PdfSharp.Drawing
Imports PdfSharp.Pdf
Imports System.Drawing.Drawing2D
Imports CONT = ConsoleDraw
Imports CONTBASE = ConsoleDraw.Windows.Base
Imports CONTINPUTS = ConsoleDraw.Inputs
Imports System.Reflection

Module Module1
    Public OriginalWindowWidth As Integer
    Public OriginalWindowHeight As Integer
    Public OriginalBufferWidth As Integer
    Public OriginalBufferHeight As Integer
    Public WithEvents pic As New PictureBox
    Public WithEvents textfillform As Form
    Public textfillformtext As String
    Public WithEvents imgdisplay As Form = New Form
    Public variables As Dictionary(Of String, String) = New Dictionary(Of String, String)
    Public currentUser As String = Environment.UserName
    Public currentTitle As String = "SETO Open Console V2 - " + currentUser + "@" + Environment.MachineName
    Public currentIsParams As Boolean = Nothing
    Public currentParams As List(Of ConsoleParameter) = New List(Of ConsoleParameter)
    Private RapidApi As RapidAPI = New RapidAPI("seto-open-console_5b8558abe4b005bfb67ae55b", "88246181-2c54-4c16-862b-ef9d282eaee9")
    Sub Main()
        StartUp()
        While True
            Dim currentInput As FormattedInput = GetCommand()
            currentIsParams = currentInput.IsParams
            currentParams = currentInput.Params
            RunCommand(currentInput.Command)
        End While
    End Sub
    Public Sub RunCommand(ByVal command As String)
        For Each value As ConsoleParameter In currentParams
            Dim matchs As MatchCollection = Regex.Matches(value.Content, "%[^\s%]+%")
            If matchs.Count > 0 Then
                For Each m As Match In matchs
                    Dim rawvar As String = m.Value.Replace("%", "")
                    Dim varvalue As String = Commands.[Get](output:=False, variable:=rawvar)
                    value.Content = value.Content.Replace(m.Value, varvalue)
                Next
            End If
        Next
        If Regex.IsMatch(command, "%[^\s%]+%") Then
            ConsoleError("Command cannot be a variable!")
            Exit Sub
        End If
        For Each value As ConsoleParameter In currentParams
            If Regex.IsMatch(value.Name, "%[^\s%]+%") Then
                ConsoleError("Parameter names cannot be variables!")
                Exit Sub
            End If
        Next
        If command.Split(" "c).Count > 1 Then
            ConsoleError("Incorrect parameter usage. '--' should always preceed a parameter!")
            Exit Sub
        End If
        Select Case command
            Case "clear"
                Console.Clear()
                Main()
            Case "exit"
                Environment.Exit(0)
            Case "title"
                Commands.Title()
            Case "echo"
                Commands.Echo()
            Case "date"
                Console.WriteLine("The current date is: " + Date.Now.Day.ToString().PadLeft(2, "0"c) + "/" + Date.Now.Month.ToString().PadLeft(2, "0"c) + "/" + Date.Now.Year.ToString())
            Case "time"
                Console.WriteLine("The current time is: " + Date.Now.Hour.ToString().PadLeft(2, "0"c) + ":" + Date.Now.Minute.ToString().PadLeft(2, "0"c) + ":" + Date.Now.Second.ToString().PadLeft(2, "0"c) + "." + Date.Now.Millisecond.ToString())
            Case "whoami"
                Console.WriteLine("You are: " + Environment.UserName)
            Case "display"
                Commands.Display()
            Case "applications"
                Commands.Applications()
            Case "ping"
                Commands.Ping()
            Case "trivia"
                Commands.Game.Trivia()
            Case "set"
                Commands.[Set]()
            Case "get"
                Commands.[Get]()
            Case "math"
                Commands.Math()
            Case "systeminfo"
                Commands.SystemInfo()
            Case "dice"
                Commands.Game.Dice()
            Case "card"
                Commands.Game.Card()
            Case "sudoku"
                Commands.Game.Sudoku()
            Case "towerofhanoi"
                Commands.Game.TowerOfHanoi()
            Case "boggle"
                Commands.Game.Boggle()
            Case "robohash"
                Commands.Misc.Robohash()
            Case "dir.create"
                Commands.Dir.Create()
            Case "dir.move"
                Commands.Dir.Move()
            Case "dir.delete"
                Commands.Dir.Delete()
            Case "xps2bmp"
                Commands.Convert.XpsToBmp()
            Case "txt2pdf"
                Commands.Convert.TxtToPdf()
            Case "getsource"
                Commands.Internet.GetSource()
            Case "shortenurl"
                Commands.Internet.ShortenUrl()
            Case "ipinfo"
                Commands.Internet.IpInfo()
            Case "lockusb"
                Commands.Utilities.LockUSB()
            Case "textfill"
                Commands.Misc.TextFill()
            Case "alert"
                Commands.Misc.Alert()
            Case "filemanager"
                Commands.FileManager()
            Case "soundcloud"
                Commands.Misc.Soundcloud()
            Case Else
                ConsoleError("'" + command + "' is not a valid command!")
        End Select
    End Sub
    Public Function YesOrNo(ByVal question As String) As Boolean
        Console.Write(question.Replace("?", "") + " (Y or N)? ")
        If Console.ReadKey().Key = ConsoleKey.Y Then
            Return True
        Else
            Return False
        End If
    End Function
    Public Function ParamExist(ByVal params As List(Of ConsoleParameter), ByVal what As String) As Boolean
        For Each p As ConsoleParameter In params
            If p.Name.ToLower().Trim() = what.ToLower().Trim() Then
                Return True
            End If
        Next
        Return False
    End Function
    Private Function GetPassword(ByVal message As String, Optional ByVal passwordMask As Char = "*"c) As String
        Dim pwd As String = String.Empty
        Dim sb As New System.Text.StringBuilder()
        Dim cki As ConsoleKeyInfo = Nothing

        'Get the password
        Console.Write(message)
        While (True)
            While Console.KeyAvailable() = False
                System.Threading.Thread.Sleep(50)
            End While
            cki = Console.ReadKey(True)
            If cki.Key = ConsoleKey.Enter Then
                Console.WriteLine()
                Exit While
            ElseIf cki.Key = ConsoleKey.Backspace Then
                If sb.Length > 0 Then
                    sb.Length -= 1
                    Console.Write(ChrW(8) & ChrW(32) & ChrW(8))
                End If
                Continue While
            ElseIf Asc(cki.KeyChar) < 32 OrElse Asc(cki.KeyChar) > 126 Then
                Continue While
            End If
            sb.Append(cki.KeyChar)
            Console.Write(passwordMask)
        End While
        pwd = sb.ToString()
        Return pwd
    End Function
    Public Sub StartUp()
        Console.Title = currentTitle
        Console.WriteLine("SETO Open Console V2.0.0 " + Date.Today)
        Console.WriteLine()
        OriginalBufferHeight = Console.BufferHeight
        OriginalBufferWidth = Console.BufferWidth
        OriginalWindowHeight = Console.WindowHeight
        OriginalWindowWidth = Console.WindowWidth
    End Sub
    Public Function GiveSelection(ByVal question As String, ByVal ParamArray Options As String()) As Integer
        Console.WriteLine(question + ":")
        Dim counter As Integer = 0
        For Each choice As String In Options
            counter += 1
            Console.WriteLine("[" + counter.ToString() + "] " + choice)
        Next
        Console.Write("> ")
        Dim chosen As Integer
        Try
            chosen = CInt(Console.ReadLine().Trim())
        Catch ex As Exception
            ConsoleError("Input must be a number!")
            Return -1
        End Try
        If chosen > Options.Count Then
            ConsoleError("Input is not in range!")
            Return -1
        End If
        Return chosen
    End Function
    Public Function GetCommand(ByVal Optional FilePath As String = "~", ByVal Optional ReturnFormattedInput As Boolean = True) As FormattedInput
        Console.ForegroundColor = ConsoleColor.Green
        Console.Write(currentUser + "@" + Environment.MachineName)
        Console.ForegroundColor = ConsoleColor.White
        Console.Write(":")
        Console.ForegroundColor = ConsoleColor.Blue
        Console.Write(FilePath)
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("$ ")
        If ReturnFormattedInput = True Then
            Return InterpretInput(Console.ReadLine(), "--")
        Else
            Return Nothing
        End If
    End Function
    Public Function Ask(ByVal question As String) As String
        Console.Write(question)
        Return Console.ReadLine().Trim()
    End Function
    Public Function GetParameterValue(ByVal params As List(Of ConsoleParameter), ByVal paramToGet As String) As String
        For Each p As ConsoleParameter In params
            If p.Name.ToLower().Trim() = paramToGet.ToLower().Trim() Then
                Return p.Content
            End If
        Next
        Return Nothing
    End Function
    Public Sub ConsoleError(ByVal message As String, ByVal Optional ExitAfter As Boolean = False)
        Console.ForegroundColor = ConsoleColor.Red
        Console.WriteLine("ERROR: " + message)
        Console.ForegroundColor = ConsoleColor.White
        If ExitAfter Then
            Console.ReadLine()
            Environment.Exit(0)
        End If
    End Sub
    Public Function VerifyCommand(ByVal params As List(Of ConsoleParameter), ByVal minParams As Integer, ByVal maxParams As Integer, ByVal ParamArray paramNames As String()) As Boolean
        If params.Count < minParams Then
            ConsoleError("This command requires at least " + minParams.ToString() + " parameter(s)!")
            Return False
        ElseIf params.Count > maxParams Then
            ConsoleError("Too many parameters. " + maxParams.ToString() + " is the maximum!")
            Return False
        Else
            Dim lowerList As String() = paramNames.Select(Function(s) s.ToLower().Trim()).ToArray()
            For Each p As ConsoleParameter In params
                If Not lowerList.Contains(p.Name.ToLower().Trim()) Then
                    ConsoleError("'" + p.Name.ToUpper() + "' is not a valid parameter!")
                    Return False
                End If
            Next
            Return True
        End If
    End Function
    Public Function InterpretInput(ByVal input As String, ByVal Optional parameterFlag As String = "//") As FormattedInput
        Dim words As String() = input.Split(New String() {parameterFlag}, StringSplitOptions.None)
        Dim result As New FormattedInput
        result.Command = words(0).ToLower().Trim()
        If words.Count = 0 Then
            words(0) = input.Trim()
        End If
        If words.Count = 1 Then
            result.IsParams = False
            result.Params = New List(Of ConsoleParameter)
        Else
            result.IsParams = True
            result.Params = New List(Of ConsoleParameter)
            For Each thing As String In words
                If thing.Trim().ToLower() = result.Command Then
                    Continue For
                End If
                Dim pToAdd As New ConsoleParameter
                Dim parts As String() = thing.Split(" "c)
                pToAdd.Name = parts(0).ToLower().Trim()
                pToAdd.Content = ""
                For Each word As String In parts
                    If word.ToLower().Trim() = pToAdd.Name Then
                        Continue For
                    End If
                    If pToAdd.Content = "" Then
                        pToAdd.Content = word.Trim()
                    Else
                        pToAdd.Content += (" " + word.Trim())
                    End If
                Next
                result.Params.Add(pToAdd)
            Next
        End If
        Return result
    End Function
    Public Class FormattedInput
        Public Command As String
        Public IsParams As Boolean
        Public Params As List(Of ConsoleParameter)
    End Class
    Public Class ConsoleParameter
        Public Name As String
        Public Content As String
    End Class
    Public Function AES_Encrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim encrypted As String = ""
        Try
            Dim hash(31) As Byte
            Dim temp As Byte() = Hash_AES.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(pass))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = System.Security.Cryptography.CipherMode.ECB
            Dim DESEncrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateEncryptor
            Dim Buffer As Byte() = System.Text.ASCIIEncoding.ASCII.GetBytes(input)
            encrypted = Convert.ToBase64String(DESEncrypter.TransformFinalBlock(Buffer, 0, Buffer.Length))
            Return encrypted
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Function AES_Decrypt(ByVal input As String, ByVal pass As String) As String
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim decrypted As String = ""
        Try
            Dim hash(31) As Byte
            Dim temp As Byte() = Hash_AES.ComputeHash(System.Text.ASCIIEncoding.ASCII.GetBytes(pass))
            Array.Copy(temp, 0, hash, 0, 16)
            Array.Copy(temp, 0, hash, 15, 16)
            AES.Key = hash
            AES.Mode = System.Security.Cryptography.CipherMode.ECB
            Dim DESDecrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateDecryptor
            Dim Buffer As Byte() = Convert.FromBase64String(input)
            decrypted = System.Text.ASCIIEncoding.ASCII.GetString(DESDecrypter.TransformFinalBlock(Buffer, 0, Buffer.Length))
            Return decrypted
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Public Sub ConsoleWarning(ByVal message As String)
        Console.ForegroundColor = ConsoleColor.Yellow
        Console.WriteLine("WARNING: " + message)
        Console.ForegroundColor = ConsoleColor.White
    End Sub
    Public Class Commands
        Public Shared Sub FileManager()
            Console.Clear()
            FileManagerFramework.Program.Main(New String() {})
            Environment.Exit(0)
        End Sub
        Public Shared Sub Title()
            If VerifyCommand(currentParams, 0, 1, "text") = False Then
                Exit Sub
            End If
            If ParamExist(currentParams, "text") Then
                currentTitle = "SETO Open Console V2 - " + GetParameterValue(currentParams, "text")
                Console.Title = currentTitle
            Else
                currentTitle = "SETO Open Console V2 - " + currentUser + "@" + Environment.MachineName
                Console.Title = currentTitle
            End If
        End Sub
        Public Shared Sub Echo()
            If VerifyCommand(currentParams, 0, 2, "m", "col") = False Then
                Exit Sub
            End If
            If ParamExist(currentParams, "m") = False Then
                Console.WriteLine()
            ElseIf ParamExist(currentParams, "m") And ParamExist(currentParams, "col") Then
                Try
                    Console.ForegroundColor = [Enum].Parse(GetType(ConsoleColor), GetParameterValue(currentParams, "col"))
                Catch ex As Exception
                    ConsoleError("Invalid Color!")
                    Exit Sub
                End Try
                Console.WriteLine(GetParameterValue(currentParams, "m"))
                Console.ForegroundColor = ConsoleColor.White
            Else
                Console.WriteLine(GetParameterValue(currentParams, "m"))
            End If
        End Sub
        Public Shared Sub Display()
            If VerifyCommand(currentParams, 0, 1, "file") = False Then
                Exit Sub
            End If
            Dim FileName As String = Nothing
            If ParamExist(currentParams, "file") Then
                FileName = GetParameterValue(currentParams, "file")
                If Not File.Exists(FileName) Then
                    ConsoleError("File does not exist!")
                    Exit Sub
                End If
            Else
                Dim ofd As New OpenFileDialog
                ofd.Title = "SETO Open Console V2 - Choose a file"
                ofd.InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
                ofd.Multiselect = False
                ofd.Filter = "Any File Type|*.*"
                ofd.FileName = ""
                If ofd.ShowDialog() = DialogResult.OK Then
                    FileName = ofd.FileName
                Else
                    ConsoleError("File is invalid!")
                    Exit Sub
                End If
            End If
            Try
                Console.WriteLine()
                Console.WriteLine("Displaying: " + FileName)
                Console.WriteLine()
                Using textReader As New StreamReader(FileName)
                    For Each line As String In textReader.ReadToEnd().Split(ControlChars.NewLine)
                        Console.WriteLine(line)
                    Next
                    Console.WriteLine()
                End Using
            Catch ex As Exception
                ConsoleError(ex.Message + "!")
                Exit Sub
            End Try
        End Sub
        Public Shared Sub Applications()
            For Each b As Process In Process.GetProcesses(".")
                Try
                    If b.MainWindowTitle.Length > 0 Then
                        Console.WriteLine("Window Title: " + b.MainWindowTitle)
                        Console.WriteLine("Process Name: " + b.ProcessName)
                        Console.WriteLine()
                    End If
                Catch ex As Exception
                    ConsoleError(ex.Message + "!")
                    Exit Sub
                End Try
            Next
        End Sub
        Public Shared Sub Ping()
            If VerifyCommand(currentParams, 1, 1, "url") = True Then
                Try
                    Dim host As String = GetParameterValue(currentParams, "url")
                    Dim pingSender As New Ping()
                    Dim options As New PingOptions()
                    Dim data As String = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"
                    Dim buffer As Byte() = Encoding.ASCII.GetBytes(data)
                    Dim timeout As Integer = 250
                    Dim reply As PingReply = pingSender.Send(host, timeout, buffer, options)
                    If reply.Status = IPStatus.Success Then
                        Console.WriteLine("Address: {0}", reply.Address.ToString())
                        Console.WriteLine("RoundTrip time: {0}", reply.RoundtripTime)
                        Console.WriteLine("Time to live: {0}", reply.Options.Ttl)
                        Console.WriteLine("Buffer size: {0}", reply.Buffer.Length)
                    End If
                Catch ex As Exception
                    ConsoleError("Error while trying to ping address! Make sure it starts with 'www.'")
                End Try
            End If
        End Sub
        Public Shared Sub [Set]()
            If ParamExist(currentParams, "clear") Then
                Dim number As Integer = variables.Count
                variables.Clear()
                Console.WriteLine("A total of " + number.ToString() + " variables were cleared")
                Exit Sub
            End If
            If VerifyCommand(currentParams, 2, 2, "name", "val") Then
                If variables.ContainsKey(GetParameterValue(currentParams, "name")) Then
                    variables.Remove(GetParameterValue(currentParams, "name"))
                End If
                Try
                    Dim n As String = GetParameterValue(currentParams, "name").Trim()
                    Dim v As String = GetParameterValue(currentParams, "val").Trim()
                    If n.Trim().Contains(" ") Then
                        ConsoleError("Variables cannot contain spaces!")
                        Exit Sub
                    End If
                    variables.Add(n, v)
                Catch ex As Exception
                    ConsoleError(ex.Message + "!")
                    Exit Sub
                End Try
                Console.WriteLine("Variable set!")
            End If
        End Sub
        Public Shared Function [Get](ByVal Optional output As Boolean = True, ByVal Optional variable As String = Nothing) As String
            If variable = Nothing Then
                If Not VerifyCommand(currentParams, 1, 1, "name") Then
                    Return Nothing
                Else
                    variable = GetParameterValue(currentParams, "name")
                End If
            End If
            If Not variables.ContainsKey(variable) Then
                ConsoleError("Variable does not exist! Use the 'set " + variable + " <VALUE>' command!")
                Return Nothing
            Else
                If output = True Then
                    Console.WriteLine(variables.Item(variable))
                    Return Nothing
                Else
                    Return variables.Item(variable)
                End If
            End If
        End Function
        Public Shared Sub Math()
            Dim currentMathIsParams As Boolean = True
            Dim currentMathParams As List(Of ConsoleParameter) = New List(Of ConsoleParameter)
            Console.WriteLine("Welcome to the Maths Console! Type 'exit' to leave")
            Console.WriteLine()
            While True
                Dim currentMathInput As FormattedInput
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.Write("Math")
                Console.ForegroundColor = ConsoleColor.White
                Console.Write("> ")
                currentMathInput = InterpretInput(Console.ReadLine().ToLower().Trim(), "--")
                currentMathIsParams = currentMathInput.IsParams
                currentMathParams = currentMathInput.Params
                For Each value As ConsoleParameter In currentMathParams
                    Dim matchs As MatchCollection = Regex.Matches(value.Content, "%[^\s%]+%")
                    If matchs.Count > 0 Then
                        For Each m As Match In matchs
                            Dim rawvar As String = m.Value.Replace("%", "")
                            Dim varvalue As String = Commands.[Get](output:=False, variable:=rawvar)
                            value.Content = value.Content.Replace(m.Value, varvalue)
                        Next
                    End If
                Next
                If Regex.IsMatch(currentMathInput.Command, "%[^\s%]+%") Then
                    ConsoleError("Command cannot be a variable!")
                    Continue While
                End If
                For Each value As ConsoleParameter In currentMathParams
                    If Regex.IsMatch(value.Name, "%[^\s%]+%") Then
                        ConsoleError("Parameter names cannot be variables!")
                        Continue While
                    End If
                Next
                If currentMathInput.Command.Split(" "c).Count > 1 Then
                    ConsoleError("Incorrect parameter usage. '--' should always preceed a parameter!")
                    Continue While
                End If
                Select Case currentMathInput.Command
                    Case "exit"
                        Exit Sub
                    Case "pfac"
                        If VerifyCommand(currentMathParams, 1, 1, "num") Then
                            If IsNumeric(GetParameterValue(currentMathParams, "num")) Then
                                Dim factors As New List(Of Integer)()
                                Dim number As Integer
                                Try
                                    number = CInt(GetParameterValue(currentMathParams, "num"))
                                Catch ex As OverflowException
                                    ConsoleError("Integer is too large!")
                                    Exit Sub
                                End Try
                                While number Mod 2 = 0
                                    factors.Add(2)
                                    number \= 2
                                End While
                                For i As Integer = 3 To System.Math.Sqrt(number) Step 2
                                    While number Mod i = 0
                                        factors.Add(i)
                                        number \= i
                                    End While
                                Next
                                If number > 2 Then
                                    factors.Add(number)
                                End If
                                Console.WriteLine("The prime factors of {0} are: ", GetParameterValue(currentMathParams, "num"))
                                For Each factor As Integer In factors
                                    Console.WriteLine(factor)
                                Next
                            Else
                                ConsoleError("'" + GetParameterValue(currentMathParams, "num") + "' is not a number!")
                                Continue While
                            End If
                        End If
                    Case "numberfact"
                        If VerifyCommand(currentMathParams, 1, 1, "num") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "num")) Then
                                ConsoleError("Input must be numeric!")
                                Continue While
                            End If
                            Console.WriteLine(WebRequests.GetRequest("http://numbersapi.com/" + GetParameterValue(currentMathParams, "num")))
                        End If
                    Case "pi"
                        If VerifyCommand(currentMathParams, 1, 1, "digits") Then
                            If IsNumeric(GetParameterValue(currentMathParams, "digits")) = False Then
                                ConsoleError("Number of digits must be given as an integer!")
                                Continue While
                            End If
                            Dim start As Integer = 0
                            Dim NoOfRequiredDigits As Integer = CInt(GetParameterValue(currentMathParams, "digits"))
                            If NoOfRequiredDigits <= 0 Then
                                ConsoleWarning("Pi to 0 digits will return nothing!")
                                Continue While
                            End If
                            Dim NoOfCurrentDigits As Integer = 0
                            While NoOfRequiredDigits > 0
                                If NoOfRequiredDigits < 1000 Then
                                    NoOfCurrentDigits = NoOfRequiredDigits
                                    NoOfRequiredDigits = 0
                                Else
                                    NoOfCurrentDigits = 1000
                                    NoOfRequiredDigits -= 1000
                                End If
                                Dim gotten As String = WebRequests.GetRequest("https://api.pi.delivery/v1/pi?start=" + start.ToString() + "&numberOfDigits=" + NoOfCurrentDigits.ToString())
                                Dim result As PI = JsonConvert.DeserializeObject(Of PI)(gotten)
                                For Each line As String In result.content.Split(ControlChars.NewLine)
                                    Console.WriteLine(line)
                                Next
                                start += 1000
                            End While
                        End If
                    Case "!"
                        If VerifyCommand(currentMathParams, 1, 1, "num") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "num")) Then
                                ConsoleError("Parameter 'num' must be numeric!")
                                Continue While
                            End If
                            Dim number As Integer = CInt(GetParameterValue(currentMathParams, "num"))
                            If number < 0 Then
                                ConsoleError("The factorial cannot be calculated for integers below zero!")
                                Continue While
                            End If
                            Try
                                Dim result As Long = 1
                                For i As Integer = 1 To number
                                    result *= i
                                Next
                                Console.WriteLine(result.ToString())
                            Catch ex As Exception
                                ConsoleError(ex.Message.Replace(".", "") + "!")
                            End Try
                        End If
                    Case "fib"
                        If VerifyCommand(currentMathParams, 1, 1, "digits") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "digits")) Then
                                ConsoleError("Parameter 'digits' must be numeric!")
                                Continue While
                            End If
                            Try
                                Dim count As Integer = CInt(GetParameterValue(currentMathParams, "digits"))
                                Dim numbers As Integer() = New Integer(count - 1) {}
                                If count > 1 Then
                                    numbers(0) = 0
                                    numbers(1) = 1
                                End If
                                Dim i As Integer = 2
                                While i < count
                                    numbers(i) = numbers(i - 1) + numbers(i - 2)
                                    i += 1
                                End While
                                Console.WriteLine("The first " + count.ToString() + " digit(s) of the Fibonacci sequence are:")
                                For Each num As Integer In numbers
                                    Console.WriteLine(num.ToString())
                                Next
                            Catch ex As Exception
                                ConsoleError(ex.Message.Replace(".", "") + "!")
                            End Try
                        End If
                    Case "gcd"
                        If VerifyCommand(currentMathParams, 2, 2, "a", "b") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "a")) Or Not IsNumeric(GetParameterValue(currentMathParams, "b")) Then
                                ConsoleError("Both the 'a' and 'b' parameters must be numeric!")
                                Continue While
                            End If
                            Dim a As Integer = CInt(GetParameterValue(currentMathParams, "a"))
                            Dim b As Integer = CInt(GetParameterValue(currentMathParams, "b"))
                            If a = 0 Then
                                Console.WriteLine(b.ToString())
                                Continue While
                            End If
                            While b <> 0
                                If a > b Then
                                    a -= b
                                Else
                                    b -= a
                                End If
                            End While
                            Console.WriteLine(a.ToString())
                            Continue While
                        End If
                    Case "lcm"
                        If VerifyCommand(currentMathParams, 2, 2, "a", "b") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "a")) Or Not IsNumeric(GetParameterValue(currentMathParams, "b")) Then
                                ConsoleError("Both the 'a' and 'b' parameters must be numeric!")
                                Continue While
                            End If
                            Dim a As Integer = CInt(GetParameterValue(currentMathParams, "a"))
                            Dim b As Integer = CInt(GetParameterValue(currentMathParams, "b"))
                            Console.WriteLine(((a * b) \ GCD(a, b)))
                        End If
                    Case "ncr"
                        If VerifyCommand(currentMathParams, 2, 2, "n", "r") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "n")) Or Not IsNumeric(GetParameterValue(currentMathParams, "r")) Then
                                ConsoleError("Both the 'n' and 'r' parameters must be numeric!")
                                Continue While
                            End If
                            Dim n As Integer = CInt(GetParameterValue(currentMathParams, "n"))
                            Dim r As Integer = CInt(GetParameterValue(currentMathParams, "r"))
                            Console.WriteLine((Factorial(n) \ (Factorial(r) * Factorial(n - r))))
                        End If
                    Case "npr"
                        If VerifyCommand(currentMathParams, 2, 2, "n", "r") Then
                            If Not IsNumeric(GetParameterValue(currentMathParams, "n")) Or Not IsNumeric(GetParameterValue(currentMathParams, "r")) Then
                                ConsoleError("Both the 'n' and 'r' parameters must be numeric!")
                                Continue While
                            End If
                            Dim n As Integer = CInt(GetParameterValue(currentMathParams, "n"))
                            Dim r As Integer = CInt(GetParameterValue(currentMathParams, "r"))
                            Console.WriteLine((Factorial(n) \ Factorial(n - r)))
                        End If
                    Case Else
                        ConsoleError("'" + currentMathInput.Command + "' is not a valid Maths Console command!")
                End Select
            End While
        End Sub
        Public Shared Sub SystemInfo()
            ConsoleWarning("This command gives an extremely long output!")
            If Not YesOrNo("Are you sure you want to continue?") Then
                Console.WriteLine()
                Exit Sub
            End If
            Console.WriteLine()
            Dim things As List(Of String) = New List(Of String) From {
                "Operating System",
                "Battery",
                "BIOS",
                "Boot Configuration",
                "USB Controller",
                "Sound Device",
                "Service",
                "Registry",
                "Product",
                "Processor",
                "Physical Memory",
                "Operating System",
                "Network Adapter",
                "Fan",
                "Environment",
                "Computer System"
            }
            For Each aspect As String In things
                Console.WriteLine("--------------- " + aspect.ToUpper() + " ---------------")
                Dim searcher As ManagementObjectSearcher = New ManagementObjectSearcher("SELECT * FROM Win32_" + aspect.Replace(" "c, ""))
                Dim queryObj As ManagementObjectCollection = searcher.Get()
                For Each mo In queryObj
                    For Each prop In mo.Properties
                        Console.WriteLine("{0} {1}", (prop.Name + ":").PadRight(30, " "c), prop.Value)
                    Next
                    Console.WriteLine()
                Next
            Next
        End Sub
        Public Class Game
            Public Shared Sub Dice()
                If VerifyCommand(currentParams, 0, 1, "sides") Then
                    If ParamExist(currentParams, "sides") Then
                        If Not IsNumeric(GetParameterValue(currentParams, "sides")) Then
                            ConsoleError("Parameter 'sides' must be numeric!")
                            Exit Sub
                        End If
                        Console.WriteLine("You rolled a " + New Random().Next(1, CInt(GetParameterValue(currentParams, "sides")) + 1).ToString() + "!")
                    Else
                        Console.WriteLine("You rolled a " + New Random().Next(1, 7).ToString() + "!")
                    End If
                End If
            End Sub
            Public Shared Sub Card()
                Dim suits As New List(Of String) From {"Hearts", "Clubs", "Spades", "Diamonds"}
                Dim numbers As New List(Of String) From {"Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King"}
                Console.WriteLine(numbers.RandomChoice() + " of " + suits.RandomChoice())
            End Sub
            Public Shared Sub Trivia()
                Try
                    Dim client As WebClient = New WebClient
                    Dim reader As StreamReader = New StreamReader(client.OpenRead("http://jservice.io/api/random"))
                    Dim gotten As String = reader.ReadToEnd()
                    Dim result As TriviaQuestion = JsonConvert.DeserializeObject(Of TriviaQuestion)(gotten.Substring(1, gotten.Length - 2))
                    Console.WriteLine()
                    Console.WriteLine("Playing for £" + result.value.ToString())
                    Console.WriteLine("Category: " + StrConv(result.category.title, VbStrConv.ProperCase))
                    Console.WriteLine()
                    Console.WriteLine("Question: " + result.question.Replace("\", ""))
                    Console.Write("Answer: ")
                    Dim inputAnswer As String = Console.ReadLine()
                    If result.answer.ToLower().Replace("\", "") = inputAnswer.ToLower().Trim() Then
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.WriteLine("CORRECT")
                    Else
                        Console.ForegroundColor = ConsoleColor.Red
                        Console.WriteLine("INCORRECT. The correct answer was " + StrConv(result.answer, VbStrConv.ProperCase))
                    End If
                    Console.ForegroundColor = ConsoleColor.White
                Catch ex As Exception
                    ConsoleError("ERROR: " + ex.Message)
                End Try
            End Sub
            Public Shared Sub Sudoku()
                Console.WriteLine("Welcome to the SETO Sudoku Solver!")
                Console.WriteLine()
                Console.WriteLine("When asked, please enter each line without spaces, substituting blanks for '0's. For example, '321704000'")
                Dim puzzle As Integer(,)
                ReDim puzzle(8, 8)
                For i = 0 To 8
                    Dim line As String = Ask("Line " + (i + 1).ToString() + ": ")
                    If line.Trim().Length <> 9 Then
                        ConsoleError("All lines must have nine digits!")
                        Exit Sub
                    End If
                    For j = 0 To 8
                        If Not IsNumeric(line(j).ToString()) Then
                            ConsoleError("'" + line(j).ToString() + "' is not numeric!")
                            Exit Sub
                        End If
                        puzzle(i, j) = CInt(line(j).ToString())
                    Next
                Next
                Console.WriteLine()
                PrintSudoku(puzzle)
                Console.WriteLine()
                If YesOrNo("Is this board correct?") Then
                    Console.WriteLine()
                Else
                    Console.WriteLine()
                    Exit Sub
                End If
                Console.Write("Please wait..." & vbCr)
                If SolveSudoku(puzzle, 0, 0) Then
                    Console.WriteLine("Sudoku Puzzle Solved!")
                    PrintSudoku(puzzle)
                Else
                    ConsoleError("Sudoku could not be solved!")
                End If
            End Sub
            Public Shared Sub TowerOfHanoi()
                If VerifyCommand(currentParams, 4, 4, "count", "from", "to", "via") Then
                    For Each param As ConsoleParameter In currentParams
                        If Not IsNumeric(param.Content) Then
                            ConsoleError("'" + param.Content + "' is not numeric!")
                            Exit Sub
                        End If
                    Next
                    Dim diskCount As Integer = CInt(GetParameterValue(currentParams, "count"))
                    Dim fromPole As Integer = CInt(GetParameterValue(currentParams, "from"))
                    Dim toPole As Integer = CInt(GetParameterValue(currentParams, "to"))
                    Dim viaPole As Integer = CInt(GetParameterValue(currentParams, "via"))
                    TowerofHanoiFunc(diskCount, fromPole, toPole, viaPole)
                End If
            End Sub
            Public Shared Sub Boggle()
                Console.WriteLine("Welcome to the SETO Boggle Solver")
                Console.WriteLine()
                Console.WriteLine("When asked, please enter each of line without spaces.")
                Dim letters As String = ""
                For i = 1 To 4
                    Dim input As String = Ask("Line " + i.ToString + ": ")
                    If input.Length <> 4 Then
                        ConsoleError("Each line must be 4 characters long!")
                        Exit Sub
                    End If
                    If Not Regex.IsMatch(input, "^[A-Za-z]+$") Then
                        ConsoleError("Input must only contain letters!")
                        Exit Sub
                    End If
                    letters += input.Trim().ToUpper()
                Next
                Console.WriteLine()
                Dim jsonResponse As Stream = Unirest.get("https://codebox-boggle-v1.p.mashape.com/" + letters).header("X-Mashape-Key", "eXLOKa2PXzmshtlSS9XvzxjgXsg5p1Jojb4jsnsL8odvnxnHUe").header("X-Mashape-Host", "codebox-boggle-v1.p.mashape.com").asJson(Of String)().Raw
                Dim array As String = New StreamReader(jsonResponse).ReadToEnd()
                Dim js As New System.Web.Script.Serialization.JavaScriptSerializer
                Dim words As String() = js.Deserialize(Of String())(array)
                Console.WriteLine(words.Length.ToString() + " words found:")
                Dim wordsOrdered As String() = words.OrderByDescending(Function(c) c.Length).ToArray()
                For Each word As String In wordsOrdered
                    Console.WriteLine(ControlChars.Tab + word.ToUpper())
                Next
            End Sub
        End Class
        Public Class Misc
            Public Shared Sub Alert()
                If VerifyCommand(currentParams, 1, 2, "message", "title") Then
                    If Not ParamExist(currentParams, "message") Then ConsoleError("The 'alert' command requires a the 'message' parameter!") : Exit Sub
                    If Not ParamExist(currentParams, "title") Then
                        Dim temp = New CONT.Windows.Alert(Nothing, GetParameterValue(currentParams, "message"))
                    Else
                        Dim temp = New CONT.Windows.Alert(Nothing, GetParameterValue(currentParams, "message"), GetParameterValue(currentParams, "title"))
                    End If
                End If
            End Sub
            Public Shared Sub Robohash()
                If VerifyCommand(currentParams, 1, 1, "text") Then
                    With imgdisplay
                        .Size = New System.Drawing.Size(350, 350)
                        .Text = "SETO Open Console V2 - Click me to save!"
                        .FormBorderStyle = FormBorderStyle.FixedSingle
                        .MaximizeBox = False
                        .MinimizeBox = False
                        .ShowInTaskbar = False
                        .ShowIcon = False
                        .StartPosition = FormStartPosition.CenterScreen
                        With pic
                            .BorderStyle = BorderStyle.FixedSingle
                            .Dock = DockStyle.Fill
                            Dim tClient As New WebClient()
                            .Image = Bitmap.FromStream(New MemoryStream(tClient.DownloadData("https://robohash.org/" + GetParameterValue(currentParams, "text").ToLower().Trim())))
                        End With
                        .Controls.Add(pic)
                    End With
                    imgdisplay.ShowDialog()
                End If
            End Sub
            Public Shared Sub TextFill()
                If VerifyCommand(currentParams, 1, 1, "text") Then
                    Dim textToFill As String = GetParameterValue(currentParams, "text")
                    If textToFill = Nothing Then
                        ConsoleError("The 'text' parameter cannot be nothing!")
                        Exit Sub
                    End If
                    textfillform = New Form()
                    With textfillform
                        .MaximizeBox = False
                        .MinimizeBox = False
                        .ShowIcon = False
                        .ShowInTaskbar = False
                        .StartPosition = FormStartPosition.CenterScreen
                        .Text = "Text Fill - " + textToFill
                        .FormBorderStyle = FormBorderStyle.FixedSingle
                        .AutoSizeMode = AutoSizeMode.GrowOnly
                    End With
                    textfillformtext = textToFill
                    textfillform.ShowDialog()
                End If
            End Sub
            Public Shared Sub Soundcloud()
                SoundcloudDownloadFramwork.Program.Main(New String() {})
            End Sub
        End Class
        Public Class Dir
            Public Shared Sub Create()
                If VerifyCommand(currentParams, 1, 1, "path") Then
                    Dim pathToCreate As String = GetParameterValue(currentParams, "path")
                    If Directory.Exists(pathToCreate) Then
                        ConsoleError("Directory already exists!")
                        Exit Sub
                    End If
                    If Not IsValidFileNameOrPath(pathToCreate) Then
                        ConsoleError("Path is invalid and so wasn't created!")
                        Exit Sub
                    End If
                    Try
                        Directory.CreateDirectory(pathToCreate)
                        Console.WriteLine("Directory Created Successfully!")
                    Catch ex As Exception
                        ConsoleError(ex.Message.Replace(".", "!"))
                    End Try
                End If
            End Sub
            Public Shared Sub Delete()
                If VerifyCommand(currentParams, 1, 1, "path") Then
                    Dim pathToDelete As String = GetParameterValue(currentParams, "path")
                    If Directory.Exists(pathToDelete) = False Then
                        ConsoleError("Directory doesn't exist!")
                        Exit Sub
                    End If
                    If Not IsValidFileNameOrPath(pathToDelete) Then
                        ConsoleError("Path is invalid and so wasn't deleted!")
                        Exit Sub
                    End If
                    Try
                        Directory.Delete(pathToDelete, True)
                        Console.WriteLine("Directory Deleted Successfully!")
                    Catch ex As Exception
                        ConsoleError(ex.Message.Replace(".", "!"))
                    End Try
                End If
            End Sub
            Public Shared Sub Move()
                If VerifyCommand(currentParams, 2, 2, "from", "to") Then
                    Dim toDir As String = GetParameterValue(currentParams, "to")
                    Dim fromDir As String = GetParameterValue(currentParams, "from")
                    If Not Directory.Exists(fromDir) Or Not Directory.Exists(toDir) Then
                        ConsoleError("Both directories must exist before executing this operation!")
                        Exit Sub
                    End If
                    If Not IsValidFileNameOrPath(toDir) Or Not IsValidFileNameOrPath(fromDir) Then
                        ConsoleError("Both directories must have valid names!")
                        Exit Sub
                    End If
                    Try
                        Directory.Move(fromDir, toDir)
                        Console.WriteLine("Directory Moved Succesfully!")
                    Catch ex As Exception
                        ConsoleError(ex.Message)
                    End Try
                End If
            End Sub
        End Class
        Public Class Convert
            Public Shared Sub XpsToBmp()
                If VerifyCommand(currentParams, 1, 1, "xps") Then
                    Dim lstOfFiles As List(Of String) = New List(Of String)
                    Dim xpsFile As String = GetParameterValue(currentParams, "xps")
                    If xpsFile = Nothing Then
                        ConsoleError("XPS File cannot be nothing!")
                        Exit Sub
                    ElseIf Not File.Exists(xpsFile) Then
                        ConsoleError("XPS File does not exist!")
                        Exit Sub
                    ElseIf Path.GetExtension(xpsFile).Replace(".", "").Trim() <> "xps" Then
                        ConsoleError("File is not in XPS format!")
                        Exit Sub
                    End If
                    If Directory.Exists("C:\SETO\OpenConsoleV2\Output\XpsToBmp\") Then
                        If Directory.GetFiles("C:\SETO\OpenConsoleV2\Output\XpsToBmp\").Length > 0 Then
                            ConsoleWarning("Output folder is not empty. If this operation continues, every file inside it will be deleted!")
                            If YesOrNo("Are you sure you want to continue?") = True Then
                                Console.WriteLine()
                                Try
                                    Directory.Delete("C:\SETO\OpenConsoleV2\Output\XpsToBmp\", True)
                                Catch ex As Exception
                                    ConsoleError(ex.Message.Replace(".", "!"))
                                    Exit Sub
                                End Try
                            Else
                                Console.WriteLine()
                                Exit Sub
                            End If
                        End If
                    End If
                    Try
                        Dim xps As New XpsDocument(xpsFile, System.IO.FileAccess.Read)
                        Dim sequence As FixedDocumentSequence = xps.GetFixedDocumentSequence()
                        For pageCount As Integer = 0 To sequence.DocumentPaginator.PageCount - 1
                            Dim page As DocumentPage = sequence.DocumentPaginator.GetPage(pageCount)
                            Dim toBitmap As New RenderTargetBitmap(CInt(page.Size.Width), CInt(page.Size.Height), 96, 96, System.Windows.Media.PixelFormats.[Default])
                            toBitmap.Render(page.Visual)
                            Dim bmpEncoder As BitmapEncoder = New BmpBitmapEncoder()
                            bmpEncoder.Frames.Add(BitmapFrame.Create(toBitmap))
                            If Not Directory.Exists("C:\SETO\OpenConsoleV2\Output\XpsToBmp\") Then
                                Directory.CreateDirectory("C:\SETO\OpenConsoleV2\Output\XpsToBmp\")
                            End If
                            Dim pathName As String = "C:\SETO\OpenConsoleV2\Output\XpsToBmp\page" & (pageCount + 1).ToString() & ".bmp"
                            lstOfFiles.Add(pathName)
                            Dim fStream As New FileStream(pathName, FileMode.Create, FileAccess.Write)
                            bmpEncoder.Save(fStream)
                            fStream.Close()
                        Next
                        Console.WriteLine("The following files were saved successfully:")
                        For Each file As String In lstOfFiles
                            Console.WriteLine(file)
                        Next
                    Catch ex As Exception
                        ConsoleError(ex.Message.Replace(".", "!"))
                        Exit Sub
                    End Try
                End If
            End Sub
            Public Shared Sub TxtToPdf()
                If VerifyCommand(currentParams, 1, 3, "txt", "title", "filename") Then
                    If Not ParamExist(currentParams, "txt") Then
                        ConsoleError("The parameter 'txt' is required!")
                        Exit Sub
                    End If
                    Dim txtPath As String = GetParameterValue(currentParams, "txt")
                    If Not Path.GetExtension(txtPath) <> ".txt" Then
                        ConsoleError("The parameter 'txt' requires a .txt file!")
                        Exit Sub
                    ElseIf Not File.Exists(txtPath) Then
                        ConsoleError("The text file doesn't exist!")
                        Exit Sub
                    End If
                    Dim titleParam As String = "Text File to PDF by SETO"
                    If ParamExist(currentParams, "title") Then
                        If IsNothing(GetParameterValue(currentParams, "title")) Then
                            ConsoleWarning("The title parameter is blank and so the default title will be used!")
                        Else
                            titleParam = GetParameterValue(currentParams, "title")
                        End If
                    End If
                    Dim filenameParam As String = "txttopdf.pdf"
                    If ParamExist(currentParams, "filename") Then
                        If IsNothing(GetParameterValue(currentParams, "filename")) Then
                            ConsoleWarning("The filename parameter is blank and so the default file name will be used!")
                        ElseIf Path.GetExtension(GetParameterValue(currentParams, "filename")) <> ".pdf" Then
                            ConsoleWarning("The filename provided is not a .pdf file and so the default file name will be used!")
                        ElseIf Not IsValidFileNameOrPath(GetParameterValue(currentParams, "filename")) Then
                            ConsoleWarning("The filename provided is invalid and so the default file name will be used!")
                        Else
                            filenameParam = GetParameterValue(currentParams, "filename")
                        End If
                    End If
                    Try
                        Dim line As String
                        Dim readFile As System.IO.TextReader = New StreamReader(txtPath)
                        Dim yPoint As Integer = 0
                        Dim pdf As PdfDocument = New PdfDocument
                        pdf.Info.CreationDate = Date.Now
                        pdf.Info.Creator = "SETO Open Console V2"
                        pdf.Info.Title = titleParam
                        Dim pdfPage As PdfPage = pdf.AddPage()
                        Dim graph As XGraphics = XGraphics.FromPdfPage(pdfPage)
                        Dim font As XFont = New XFont("Verdana", 20, XFontStyle.Regular)
                        While True
                            line = readFile.ReadLine()
                            If line Is Nothing Then
                                Exit While
                            Else
                                graph.DrawString(line, font, XBrushes.Black, New XRect(40, yPoint, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft)
                                yPoint = yPoint + 40
                            End If
                        End While
                        If Not Directory.Exists("C:\SETO\OpenConsoleV2\Output\TxtToPdf\") Then
                            Directory.CreateDirectory("C:\SETO\OpenConsoleV2\Output\TxtToPdf\")
                        End If
                        pdf.Save("C:\SETO\OpenConsoleV2\Output\TxtToPdf\" + filenameParam)
                        readFile.Close()
                        readFile = Nothing
                        Console.WriteLine("File converted successfully!")
                        If YesOrNo("Would you like to view?") = True Then
                            Console.WriteLine()
                            Process.Start("C:\SETO\OpenConsoleV2\Output\TxtToPdf\" + filenameParam)
                        Else
                            Console.WriteLine()
                            Exit Sub
                        End If
                    Catch ex As Exception
                        ConsoleError(ex.Message.Replace(".", "!"))
                    End Try
                End If
            End Sub
        End Class
        Public Class Internet
            Public Shared Sub GetSource()
                If VerifyCommand(currentParams, 1, 1, "url") Then
                    Dim url As String = GetParameterValue(currentParams, "url")
                    If Not Regex.IsMatch(url, "http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?") Then
                        ConsoleError("URL is invalid! Make sure it starts with 'http://'")
                    End If
                    Try
                        Dim webRequest As WebRequest = WebRequest.Create(url)
                        Dim webresponse As WebResponse = webRequest.GetResponse()
                        Dim inStream As StreamReader = New StreamReader(webresponse.GetResponseStream())
                        Dim response As String = inStream.ReadToEnd()
                        Console.WriteLine("Source Code Fetch Complete!")
                        Console.WriteLine()
                        For Each line As String In response.Split(ControlChars.NewLine)
                            Console.WriteLine(line)
                        Next
                    Catch ex As Exception
                        ConsoleError(ex.Message.Replace(".", "!"))
                    End Try
                End If
            End Sub
            Public Shared Sub ShortenUrl()
                If VerifyCommand(currentParams, 1, 1, "url") Then
                    Dim url As String = GetParameterValue(currentParams, "url")
                    If Not Regex.IsMatch(url, "http(s)?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\#\$\%\^\&\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?") Then
                        ConsoleError("URL is invalid! Make sure it starts with 'http://'")
                    End If
                    If My.Computer.Network.IsAvailable Then
                        Try
                            Console.WriteLine(WebRequests.GetRequest("http://tinyurl.com/api-create.php?url=" + url))
                        Catch ex As Exception
                            ConsoleError(ex.Message.Replace(".", "!"))
                        End Try
                    Else
                        ConsoleError("An internet connection is required to shorten a url!")
                        Exit Sub
                    End If
                End If
            End Sub
            Public Shared Sub IpInfo()
                If Not VerifyCommand(currentParams, 1, 1, "ip") Then
                    Exit Sub
                End If
                Dim IP As String = GetParameterValue(currentParams, "ip")
                Dim test As IPAddress = Nothing
                If Not IPAddress.TryParse(IP, test) Then
                    ConsoleError("IP address is invalid!")
                    Exit Sub
                End If
                Console.WriteLine("")
                Dim IP_Array() As String = IP.Split(".")

                Dim First_Byte As Integer = IP_Array(0)
                Dim Bit_First_Byte As String = System.Convert.ToString(First_Byte, 2)
                Dim Bit_First_Byte_Padded As String = Bit_First_Byte.PadLeft(8, "0")

                If Bit_First_Byte_Padded.Substring(0, 1) = "0" Then
                    class_X(IP_Array, "A", "11111111.00000000.00000000.00000000", "00000000.11111111.11111111.11111111")
                ElseIf Bit_First_Byte_Padded.Substring(0, 2) = "10" Then
                    class_X(IP_Array, "B", "11111111.11111111.00000000.00000000", "00000000.00000000.11111111.11111111")
                ElseIf Bit_First_Byte_Padded.Substring(0, 2) = "11" Then
                    class_X(IP_Array, "C", "11111111.11111111.11111111.00000000", "00000000.00000000.00000000.11111111")
                End If

                Console.ReadKey()
            End Sub
        End Class
        Public Class Utilities
            Public Shared Sub LockUSB()
                If VerifyCommand(currentParams, 1, 1, "lock", "unlock") Then
                    If ParamExist(currentParams, "lock") Then
                        Try
                            My.Computer.Registry.SetValue("HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\StorageDevicePolicies", "WriteProtect", "00000001", Microsoft.Win32.RegistryValueKind.DWord)
                            Console.WriteLine("USB Slot Locked!")
                        Catch ex As Exception
                            ConsoleError(ex.Message.Replace(".", "!"))
                        End Try
                    Else
                        Try
                            My.Computer.Registry.SetValue("HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\StorageDevicePolicies", "WriteProtect", "00000000", Microsoft.Win32.RegistryValueKind.DWord)
                            Console.WriteLine("USB Slot Unlocked!")
                        Catch ex As Exception
                            ConsoleError(ex.Message.Replace(".", "!"))
                        End Try
                    End If
                End If
            End Sub
        End Class
    End Class
    Function IsValidFileNameOrPath(ByVal name As String) As Boolean
        ' Determines if the name is Nothing.
        If name Is Nothing Then
            Return False
        End If

        ' Determines if there are bad characters in the name.
        For Each badChar As Char In System.IO.Path.GetInvalidPathChars
            If InStr(name, badChar) > 0 Then
                Return False
            End If
        Next

        ' The name passes basic validation.
        Return True
    End Function
    Public Sub pic_Click() Handles pic.Click
        With New SaveFileDialog()
            .AddExtension = True
            .CheckPathExists = True
            .FileName = ""
            .Filter = "Bitmap Image|*.bmp|PNG Image|*.png|JPG Image|*.jpg"
            .InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyPictures
            .Title = "SETO Open Console V2 - Save Image As..."
            If .ShowDialog() = DialogResult.OK Then
                Try
                    pic.Image.Save(.FileName)
                    imgdisplay.Hide()
                    If YesOrNo("File Saved Successfully! Would you like to view?") Then
                        Console.WriteLine()
                        Try
                            Process.Start(.FileName)
                        Catch ex As Exception
                            ConsoleError(ex.Message.Replace(".", "!"))
                        End Try
                    Else
                        Console.WriteLine()
                    End If
                Catch ex As Exception
                    ConsoleError(ex.Message.Replace(".", "!"))
                End Try
            Else
                ConsoleError("File couldn't be saved!")
            End If
        End With
    End Sub
    Public Sub TowerofHanoiFunc(diskCount As Integer, fromPole As Integer, toPole As Integer, viaPole As Integer)
        If diskCount = 1 Then
            System.Console.WriteLine("Move disk from pole " & fromPole & " to pole " & toPole)
        Else
            TowerofHanoiFunc(diskCount - 1, fromPole, viaPole, toPole)
            TowerofHanoiFunc(1, fromPole, toPole, viaPole)
            TowerofHanoiFunc(diskCount - 1, viaPole, toPole, fromPole)
        End If
    End Sub
    <Extension()>
    Public Function RandomChoice(Of T)(ByVal lst As List(Of T)) As T
        Return lst(New Random().Next(0, lst.Count - 1))
    End Function
    Public Class Category
        Public Property id As Integer
        Public Property title As String
        Public Property created_at As DateTime
        Public Property updated_at As DateTime
        Public Property clues_count As Integer
    End Class
    Private Function Factorial(number As Integer) As Long
        If number < 0 Then
            Return -1 'Error
        End If

        Dim result As Long = 1

        For i As Integer = 1 To number
            result *= i
        Next

        Return result
    End Function
    Private Function GCD(a As Integer, b As Integer) As Integer
        If a = 0 Then
            Return b
        End If

        While b <> 0
            If a > b Then
                a -= b
            Else
                b -= a
            End If
        End While

        Return a
    End Function
    Public Class TriviaQuestion
        Public Property id As Integer
        Public Property answer As String
        Public Property question As String
        Public Property value As Integer
        Public Property airdate As DateTime
        Public Property created_at As DateTime
        Public Property updated_at As DateTime
        Public Property category_id As Integer
        Public Property game_id As Object
        Public Property invalid_count As Object
        Public Property category As Category
    End Class
    Public Class WebRequests
        Public Shared Function GetRequest(ByVal url As String) As String
            Dim wbclient As WebClient = New WebClient()
            Dim strmReader As StreamReader = New StreamReader(wbclient.OpenRead(url))
            Return strmReader.ReadToEnd()
        End Function
    End Class
    Public Class PI
        Public Property content As String
    End Class
    Public Sub PrintSudoku(puzzle As Integer(,))
        Console.WriteLine("+-----+-----+-----+")

        For i As Integer = 1 To 9
            For j As Integer = 1 To 9
                Console.Write("|{0}", puzzle(i - 1, j - 1).ToString().Replace("0", " "))
            Next

            Console.WriteLine("|")
            If i Mod 3 = 0 Then
                Console.WriteLine("+-----+-----+-----+")
            End If
        Next
    End Sub
    Public Function SolveSudoku(puzzle As Integer(,), row As Integer, col As Integer) As Boolean
        If row < 9 AndAlso col < 9 Then
            If puzzle(row, col) <> 0 Then
                If (col + 1) < 9 Then
                    Return SolveSudoku(puzzle, row, col + 1)
                ElseIf (row + 1) < 9 Then
                    Return SolveSudoku(puzzle, row + 1, 0)
                Else
                    Return True
                End If
            Else
                For i As Integer = 0 To 8
                    If IsAvailable(puzzle, row, col, i + 1) Then
                        puzzle(row, col) = i + 1

                        If (col + 1) < 9 Then
                            If SolveSudoku(puzzle, row, col + 1) Then
                                Return True
                            Else
                                puzzle(row, col) = 0
                            End If
                        ElseIf (row + 1) < 9 Then
                            If SolveSudoku(puzzle, row + 1, 0) Then
                                Return True
                            Else
                                puzzle(row, col) = 0
                            End If
                        Else
                            Return True
                        End If
                    End If
                Next
            End If

            Return False
        Else
            Return True
        End If
    End Function
    Private Function IsAvailable(puzzle As Integer(,), row As Integer, col As Integer, num As Integer) As Boolean
        Dim rowStart As Integer = (row \ 3) * 3
        Dim colStart As Integer = (col \ 3) * 3

        For i As Integer = 0 To 8
            If puzzle(row, i) = num Then
                Return False
            End If
            If puzzle(i, col) = num Then
                Return False
            End If
            If puzzle(rowStart + (i Mod 3), colStart + (i \ 3)) = num Then
                Return False
            End If
        Next

        Return True
    End Function
    Public Class SolvedBoggle
        Public body As String()
    End Class
    Sub class_X(ByVal IP() As String, ByVal Classe As String, ByVal SubMask As String, ByVal SubMaskInv As String)
        Console.WriteLine("******************************************************")
        Console.WriteLine("Class: " & Classe)
        Console.WriteLine("")
        If Classe = "A" Then
            Console.WriteLine("Subnet Mask: 255.0.0.0")
        ElseIf Classe = "B" Then
            Console.WriteLine("Subnet Mask: 255.255.0.0")
        ElseIf Classe = "C" Then
            Console.WriteLine("Subnet Mask: 255.255.255.0")
        End If

        Console.WriteLine("")
        Dim Binary_IP As String = ""
        For i = 0 To IP.Length - 1
            Dim Byte_ As Integer = IP(i)
            Dim Bit As String = Convert.ToString(Byte_, 2)
            Dim Bit_Padded As String = Bit.PadLeft(8, "0")
            If Not i = IP.Length - 1 Then
                Binary_IP += Bit_Padded & "."
            Else
                Binary_IP += Bit_Padded
            End If
        Next
        Console.WriteLine("IP Binary: " & Binary_IP)
        Console.WriteLine("")

        Dim Subnet_Mask_Binary As String = SubMask
        Dim Mask_Array() As Char = Subnet_Mask_Binary.ToCharArray
        Dim IP_Array() As Char = Binary_IP.ToCharArray
        Dim Network_Address_Binary As String = ""
        For i = 0 To Mask_Array.Length - 1
            If IP_Array(i) = "1" And Mask_Array(i) = "1" Then
                Network_Address_Binary += "1"
            ElseIf IP_Array(i) = "." Then
                Network_Address_Binary += "."
            Else
                Network_Address_Binary += "0"
            End If
        Next
        Dim Network_Address As String = ""
        Dim Network_Array() As String = Network_Address_Binary.Split(".")
        For i = 0 To Network_Array.Length - 1
            Network_Address += Bin_To_Dec(Network_Array(i)) & "."
        Next
        Console.WriteLine("Network Address: " & Network_Address.Substring(0, Network_Address.Length - 1))
        Console.WriteLine("")

        Dim Subnet_Mask_Inverted As String = SubMaskInv
        Dim Mask_Inverted_Array() As Char = Subnet_Mask_Inverted.ToCharArray
        Dim Broadcast_Address_Binary As String = ""
        For i = 0 To Mask_Inverted_Array.Length - 1
            If IP_Array(i) = "0" And Mask_Inverted_Array(i) = "0" Then
                Broadcast_Address_Binary += "0"
            ElseIf IP_Array(i) = "." Then
                Broadcast_Address_Binary += "."
            Else
                Broadcast_Address_Binary += "1"
            End If
        Next
        Dim Broadcast_Address As String = ""
        Dim Broadcast_Array() As String = Broadcast_Address_Binary.Split(".")
        For i = 0 To Broadcast_Array.Length - 1
            Broadcast_Address += Bin_To_Dec(Broadcast_Array(i)) & "."
        Next
        Console.WriteLine("Broadcast Address: " & Broadcast_Address.Substring(0, Broadcast_Address.Length - 1))
        Console.WriteLine("******************************************************")
    End Sub
    Public Function Bin_To_Dec(ByVal Bin As String)
        Dim dec As Double = Nothing
        Dim length As Integer = Len(Bin)
        Dim temp As Integer = Nothing
        Dim x As Integer = Nothing
        For x = 1 To length
            temp = Val(Mid(Bin, length, 1))
            length = length - 1
            If temp <> "0" Then
                dec += (2 ^ (x - 1))
            End If
        Next
        Return dec
    End Function
    Private Sub textfillform_Paint(sender As Object, e As PaintEventArgs) Handles textfillform.Paint
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Dim path As GraphicsPath = New GraphicsPath(FillMode.Alternate)
        Using font_family As FontFamily = New FontFamily("Times New Roman")
            Using sf As StringFormat = New StringFormat()
                sf.Alignment = StringAlignment.Center
                sf.LineAlignment = StringAlignment.Center
                path.AddString(textfillformtext, font_family, CInt(FontStyle.Bold), 200, textfillform.ClientRectangle, sf)
            End Using
        End Using
        Using small_font As Font = New Font("Times New Roman", 8)
            Dim text_size As SizeF = e.Graphics.MeasureString(textfillformtext, small_font)
            Dim bm As Bitmap = New Bitmap(CInt(2 * text_size.Width), CInt(2 * text_size.Height))
            Using gr As Graphics = Graphics.FromImage(bm)
                gr.Clear(Color.LightBlue)
                gr.DrawString(textfillformtext, small_font, Brushes.Red, 0, 0)
                gr.DrawString(textfillformtext, small_font, Brushes.Green, text_size.Width, 0)
                gr.DrawString(textfillformtext, small_font, Brushes.Blue, -text_size.Width / 2, text_size.Height)
                gr.DrawString(textfillformtext, small_font, Brushes.DarkOrange, text_size.Width / 2, text_size.Height)
                gr.DrawString(textfillformtext, small_font, Brushes.Blue, 1.5F * text_size.Width, text_size.Height)
            End Using
            Using br As TextureBrush = New TextureBrush(bm)
                e.Graphics.FillPath(br, path)
            End Using
        End Using
        Using pen As Pen = New Pen(Color.Black, 3)
            e.Graphics.DrawPath(pen, path)
        End Using
    End Sub
End Module
Namespace PsCon
    Structure FrameLine
        Public topLeft As String
        Public topRight As String
        Public bottomLeft As String
        Public bottomRight As String
        Public lineX As String
        Public lineY As String
        Public Sub New(ByVal n As Integer)
            topLeft = "┌"
            topRight = "┐"
            bottomLeft = "└"
            bottomRight = "┘"
            lineX = "─"
            lineY = "│"
        End Sub
    End Structure
    Structure FrameDoubleLine
        Public topLeft As String
        Public topRight As String
        Public bottomLeft As String
        Public bottomRight As String
        Public lineA As String
        Public lineB As String
        Public Sub New(ByVal n As Integer)
            topLeft = "╔"
            topRight = "╗"
            bottomLeft = "╚"
            bottomRight = "╝"
            lineA = "═"
            lineB = "║"
        End Sub
    End Structure
    Structure Square
        Public model1 As String
        Public model2 As String
        Public model3 As String
        Public model4 As String
        Public model5 As String

        Public Sub New(ByVal n As Integer)
            model1 = "■"
            model2 = "█"
            model3 = "▓"
            model4 = "▒"
            model5 = "░"
        End Sub
    End Structure
    Structure HorizontalLine
        Public left As String
        Public right As String
        Public line As String
        Public cross As String

        Public Sub New(ByVal n As Integer)
            left = "├"
            right = "┤"
            line = "─"
            cross = "┼"
        End Sub
    End Structure
    Structure HorizontalLineDouble
        Public left As String
        Public right As String
        Public line As String
        Public cross As String

        Public Sub New(ByVal n As Integer)
            left = "╠"
            right = "╣"
            line = "═"
            cross = "╬"
        End Sub
    End Structure
    Structure VerticalLine
        Public top As String
        Public bottom As String
        Public line As String
        Public cross As String

        Public Sub New(ByVal n As Integer)
            top = "┬"
            bottom = "┴"
            line = "│"
            cross = "┼"
        End Sub
    End Structure
    Structure VerticalLineDouble
        Public top As String
        Public bottom As String
        Public line As String
        Public cross As String

        Public Sub New(ByVal n As Integer)
            top = "╦"
            bottom = "╩"
            line = "║"
            cross = "╬"
        End Sub
    End Structure
    Module PsCon
        Public Sub OpenBuffer(ByVal bufferName As String, ByVal bufferSizeX As Integer, ByVal bufferSizeY As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Console.Title = bufferName
            Console.SetWindowSize(bufferSizeX, bufferSizeY)
            Console.ForegroundColor = text
            Console.BackgroundColor = background
            Console.Clear()
            Console.SetCursorPosition(0, 0)
        End Sub
        Public Sub PrintFrameLine(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal sizeY As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            sizeY = positionY + sizeY
            Dim f As FrameLine = New FrameLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To SizeY - 1

                For x As Integer = positionX To SizeX - 1

                    If y = positionY AndAlso x = positionX Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.topLeft)
                    End If

                    If y = positionY AndAlso x > positionX AndAlso x < SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineX)
                    End If

                    If y = positionY AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.topRight)
                    End If

                    If y > positionY AndAlso y < SizeY - 1 AndAlso x = positionX OrElse y > positionY AndAlso y < SizeY - 1 AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineY)
                    End If

                    If y = SizeY - 1 AndAlso x = positionX Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.bottomLeft)
                    End If

                    If y = SizeY - 1 AndAlso x > positionX AndAlso x < SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineX)
                    End If

                    If y = SizeY - 1 AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.bottomRight)
                    End If
                Next
            Next

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintFrameDoubleLine(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal sizeY As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeY = positionY + sizeY
            sizeX = positionX + sizeX
            Dim f As FrameDoubleLine = New FrameDoubleLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To SizeY - 1

                For x As Integer = positionX To SizeX - 1

                    If y = positionY AndAlso x = positionX Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.topLeft)
                    End If

                    If y = positionY AndAlso x > positionX AndAlso x < SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineA)
                    End If

                    If y = positionY AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.topRight)
                    End If

                    If y > positionY AndAlso y < SizeY - 1 AndAlso x = positionX OrElse y > positionY AndAlso y < SizeY - 1 AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineB)
                    End If

                    If y = SizeY - 1 AndAlso x = positionX Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.bottomLeft)
                    End If

                    If y = SizeY - 1 AndAlso x > positionX AndAlso x < SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.lineA)
                    End If

                    If y = SizeY - 1 AndAlso x = SizeX - 1 Then
                        Console.SetCursorPosition(x, y)
                        Console.Write(f.bottomRight)
                    End If
                Next
            Next

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintSquare(ByVal model As Integer, ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal sizeY As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            sizeY = positionY + sizeY
            Dim sq As Square = New Square(0)
            Dim square As String = "Error!"
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            Select Case model
                Case 0
                    square = sq.model1
                Case 1
                    square = sq.model2
                Case 2
                    square = sq.model3
                Case 3
                    square = sq.model4
                Case 4
                    square = sq.model5
            End Select

            For y As Integer = positionY To SizeY - 1

                For x As Integer = positionX To SizeX - 1
                    Console.SetCursorPosition(x, y)
                    Console.Write(square)
                Next
            Next

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintHorizontalLine(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            Dim hdl As HorizontalLine = New HorizontalLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For x As Integer = positionX To SizeX - 1

                If x = positionX Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.left)
                End If

                If x > positionX AndAlso x < SizeX - 1 Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.line)
                End If

                If x = SizeX - 1 Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.right)
                End If
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(cross, positionY)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintHorizontalLine(ByVal cansel As Boolean, ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            Dim hdl As HorizontalLine = New HorizontalLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For x As Integer = positionX To SizeX - 1
                Console.SetCursorPosition(x, positionY)
                Console.Write(hdl.line)
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(cross, positionY)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintHorizontalDoubleLine(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            Dim hdl As HorizontalLineDouble = New HorizontalLineDouble(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For x As Integer = positionX To SizeX - 1

                If x = positionX Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.left)
                End If

                If x > positionX AndAlso x < SizeX - 1 Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.line)
                End If

                If x = SizeX - 1 Then
                    Console.SetCursorPosition(x, positionY)
                    Console.Write(hdl.right)
                End If
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(cross, positionY)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintHorizontalDoubleLine(ByVal cansel As Boolean, ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeX As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeX = positionX + sizeX
            Dim hdl As HorizontalLineDouble = New HorizontalLineDouble(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For x As Integer = positionX To SizeX - 1
                Console.SetCursorPosition(x, positionY)
                Console.Write(hdl.line)
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(cross, positionY)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintVerticalLine(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeY As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeY = positionY + sizeY
            Dim hdl As VerticalLine = New VerticalLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To sizeY - 1

                If y = positionY Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.top)
                End If

                If y > positionY AndAlso y < sizeY - 1 Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.line)
                End If

                If y = sizeY - 1 Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.bottom)
                End If
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(positionX, cross)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintVerticalLine(ByVal cansel As Boolean, ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeY As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeY = positionY + sizeY
            Dim hdl As VerticalLine = New VerticalLine(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To sizeY - 1
                Console.SetCursorPosition(positionX, y)
                Console.Write(hdl.line)
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(positionX, cross)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintVerticalLineDouble(ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeY As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeY = positionY + sizeY
            Dim hdl As VerticalLineDouble = New VerticalLineDouble(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To SizeY - 1

                If y = positionY Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.top)
                End If

                If y > positionY AndAlso y < SizeY - 1 Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.line)
                End If

                If y = SizeY - 1 Then
                    Console.SetCursorPosition(positionX, y)
                    Console.Write(hdl.bottom)
                End If
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(positionX, cross)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintVerticalLineDouble(ByVal cansel As Boolean, ByVal positionX As Integer, ByVal positionY As Integer, ByVal sizeY As Integer, ByVal cross As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            sizeY = positionY + sizeY
            Dim hdl As VerticalLineDouble = New VerticalLineDouble(0)
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To SizeY - 1
                Console.SetCursorPosition(positionX, y)
                Console.Write(hdl.line)
            Next

            If cross <> -1 Then
                Console.SetCursorPosition(positionX, cross)
                Console.Write(hdl.cross)
            End If

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintCountChar(ByVal ch As String, ByVal positionX As Integer, ByVal positionY As Integer, ByVal size As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Dim SizeX As Integer = positionX + size
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For x As Integer = positionX To SizeX - 1
                Console.SetCursorPosition(x, positionY)
                Console.Write(ch)
            Next

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintCountChar(ByVal cansel As Boolean, ByVal ch As String, ByVal positionX As Integer, ByVal positionY As Integer, ByVal size As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Dim SizeY As Integer = positionY + size
            Console.ForegroundColor = text
            Console.BackgroundColor = background

            For y As Integer = positionY To SizeY - 1
                Console.SetCursorPosition(positionX, y)
                Console.Write(ch)
            Next

            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintString(ByVal str As String, ByVal X As Integer, ByVal Y As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Console.ForegroundColor = text
            Console.BackgroundColor = background
            Console.SetCursorPosition(X, Y)
            Console.Write(str)
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintCount(ByVal count As Integer, ByVal X As Integer, ByVal Y As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Console.ForegroundColor = text
            Console.BackgroundColor = background
            Console.SetCursorPosition(X, Y)
            Console.Write(count)
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub PrintDouble(ByVal count As Double, ByVal X As Integer, ByVal Y As Integer, ByVal text As ConsoleColor, ByVal background As ConsoleColor)
            Console.ForegroundColor = text
            Console.BackgroundColor = background
            Console.SetCursorPosition(X, Y)
            Console.Write(count)
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Function Rand() As Integer
            Dim point As Integer = 0
            Dim r As Random = New Random()
            point = r.Next(0, 100)
            Return point
        End Function
        Public Function Rand(ByVal min As Integer, ByVal max As Integer) As Integer
            Dim point As Integer = 0
            Dim r As Random = New Random()
            point = r.Next(min, max)
            Return point
        End Function
    End Module
End Namespace
Namespace FileManagerFramework
    Public Delegate Sub OnKey(ByVal key As ConsoleKeyInfo)
    Class Program
        Shared Sub Main(ByVal args As String())
            Dim manager As FileManager = New FileManager()
            manager.Explore()
        End Sub
    End Class
    Class FileManager
        Public Shared HEIGHT_KEYS As Integer = 3
        Public Shared BOTTOM_OFFSET As Integer = 2
        Public Event KeyPress As OnKey
        Public panels As List(Of FilePanel) = New List(Of FilePanel)()
        Private activePanelIndex As Integer
        Shared Sub New()
            Console.CursorVisible = False
            Console.SetWindowSize(120, 41)
            Console.SetBufferSize(120, 41)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.BackgroundColor = ConsoleColor.Black
        End Sub
        Public Sub New()
            Dim filePanel As FilePanel = New FilePanel()
            filePanel.Top = 0
            filePanel.Left = 0
            Me.panels.Add(filePanel)
            filePanel = New FilePanel()
            filePanel.Top = FilePanel.PANEL_HEIGHT
            filePanel.Left = 0
            Me.panels.Add(filePanel)
            activePanelIndex = 0
            Me.panels(Me.activePanelIndex).Active = True
            AddHandler KeyPress, AddressOf Me.panels(Me.activePanelIndex).KeyboardProcessing
            For Each fp As FilePanel In panels
                fp.Show()
            Next
            Me.ShowKeys()
        End Sub
        Public Sub Explore()
            Dim [exit] As Boolean = False
            While Not [exit]
                If Console.KeyAvailable Then
                    Me.ClearMessage()
                    Dim userKey As ConsoleKeyInfo = Console.ReadKey(True)
                    Select Case userKey.Key
                        Case ConsoleKey.Tab
                            Me.ChangeActivePanel()
                        Case ConsoleKey.Enter
                            Me.ChangeDirectoryOrRunProcess()
                        Case ConsoleKey.F3
                            Me.ViewFile()
                        Case ConsoleKey.F4
                            Me.FindFile()
                        Case ConsoleKey.F5
                            Me.Copy()
                        Case ConsoleKey.F6
                            Me.Move()
                        Case ConsoleKey.F7
                            Me.CreateDirectory()
                        Case ConsoleKey.F8
                            Me.Rename()
                        Case ConsoleKey.F9
                            Me.Delete()
                        Case ConsoleKey.F10
                            [exit] = True
                            Console.ResetColor()
                            Console.Clear()
                            Exit Select
                        Case ConsoleKey.DownArrow
                            RaiseEvent KeyPress(userKey)
                        Case ConsoleKey.UpArrow
                            RaiseEvent KeyPress(userKey)
                        Case ConsoleKey.[End]
                            RaiseEvent KeyPress(userKey)
                        Case ConsoleKey.Home
                            RaiseEvent KeyPress(userKey)
                        Case ConsoleKey.PageDown
                            RaiseEvent KeyPress(userKey)
                        Case ConsoleKey.PageUp
                            RaiseEvent KeyPress(userKey)
                        Case Else
                            Exit Select
                    End Select
                End If
            End While
        End Sub
        Private Function AksName(ByVal message As String) As String
            Dim name As String = Nothing
            Console.CursorVisible = True
            Do
                Me.ClearMessage()
                Me.ShowMessage(message)
                name = Console.ReadLine()
            Loop While name.Length = 0
            Console.CursorVisible = False
            Me.ClearMessage()
            Return name
        End Function
        Private Sub Copy()
            For Each panel As FilePanel In panels
                If panel.isDiscs Then Return
            Next
            If Me.panels(0).Path = Me.panels(1).Path Then Return
            Try
                Dim destPath As String = If(Me.activePanelIndex = 0, Me.panels(1).Path, Me.panels(0).Path)
                Dim fileObject As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()
                Dim currentFile As FileInfo = TryCast(fileObject, FileInfo)
                If currentFile IsNot Nothing Then
                    Dim fileName As String = currentFile.Name
                    Dim destName As String = Path.Combine(destPath, fileName)
                    File.Copy(currentFile.FullName, destName, True)
                Else
                    Dim currentDir As String = (CType(fileObject, DirectoryInfo)).FullName
                    Dim destDir As String = Path.Combine(destPath, (CType(fileObject, DirectoryInfo)).Name)
                    CopyDirectory(currentDir, destDir)
                End If
                Me.RefreshPannels()
            Catch e As Exception
                Me.ShowMessage(e.Message)
                Return
            End Try
        End Sub
        Private Sub CopyDirectory(ByVal sourceDirName As String, ByVal destDirName As String)
            Dim dir As DirectoryInfo = New DirectoryInfo(sourceDirName)
            Dim dirs As DirectoryInfo() = dir.GetDirectories()

            If Not Directory.Exists(destDirName) Then
                Directory.CreateDirectory(destDirName)
            End If

            Dim files As FileInfo() = dir.GetFiles()

            For Each file As FileInfo In files
                Dim temppath As String = Path.Combine(destDirName, file.Name)
                file.CopyTo(temppath, True)
            Next

            For Each subdir As DirectoryInfo In dirs
                Dim temppath As String = Path.Combine(destDirName, subdir.Name)
                CopyDirectory(subdir.FullName, temppath)
            Next
        End Sub
        Private Sub Delete()
            If Me.panels(Me.activePanelIndex).isDiscs Then Return
            Dim fileObject As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()
            Try

                If TypeOf fileObject Is DirectoryInfo Then
                    CType(fileObject, DirectoryInfo).Delete(True)
                Else
                    CType(fileObject, FileInfo).Delete()
                End If
                Me.RefreshPannels()
            Catch e As Exception
                Me.ShowMessage(e.Message)
                Return
            End Try
        End Sub
        Private Sub CreateDirectory()
            If Me.panels(Me.activePanelIndex).isDiscs Then
                Return
            End If

            Dim destPath As String = Me.panels(Me.activePanelIndex).Path
            Dim dirName As String = Me.AksName("Enter directory name: ")

            Try
                Dim dirFullName As String = Path.Combine(destPath, dirName)
                Dim dir As DirectoryInfo = New DirectoryInfo(dirFullName)

                If Not dir.Exists Then
                    dir.Create()
                Else
                    Me.ShowMessage("A directory with the same name already exists.")
                End If

                Me.RefreshPannels()
            Catch e As Exception
                Me.ShowMessage(e.Message)
            End Try
        End Sub
        Private Sub Move()
            For Each panel As FilePanel In panels

                If panel.isDiscs Then
                    Return
                End If
            Next

            If Me.panels(0).Path = Me.panels(1).Path Then
                Return
            End If

            Try
                Dim destPath As String = If(Me.activePanelIndex = 0, Me.panels(1).Path, Me.panels(0).Path)
                Dim fileObject As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()
                Dim objectName As String = fileObject.Name
                Dim destName As String = Path.Combine(destPath, objectName)

                If TypeOf fileObject Is FileInfo Then
                    CType(fileObject, FileInfo).MoveTo(destName)
                Else
                    CType(fileObject, DirectoryInfo).MoveTo(destName)
                End If

                Me.RefreshPannels()
            Catch e As Exception
                Me.ShowMessage(e.Message)
                Return
            End Try
        End Sub
        Private Sub Rename()
            If Me.panels(Me.activePanelIndex).isDiscs Then
                Return
            End If

            Dim fileObject As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()
            Dim currentPath As String = Me.panels(Me.activePanelIndex).Path
            Dim newName As String = Me.AksName("Enter new name: ")
            Dim newFullName As String = Path.Combine(currentPath, newName)
            Try
                If TypeOf fileObject Is FileInfo Then
                    CType(fileObject, FileInfo).MoveTo(newFullName)
                Else
                    CType(fileObject, DirectoryInfo).MoveTo(newFullName)
                End If
                Me.RefreshPannels()
            Catch e As Exception
                Me.ShowMessage(e.Message)
            End Try
        End Sub
        Private Sub ViewFile()
            If Me.panels(Me.activePanelIndex).isDiscs Then
                Return
            End If

            Dim fileObject As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()

            If TypeOf fileObject Is DirectoryInfo OrElse fileObject Is Nothing Then
                Return
            End If

            If (CType(fileObject, FileInfo)).Length = 0 Then
                Me.ShowMessage("File is empty")
                Return
            End If

            If (CType(fileObject, FileInfo)).Length > 100000000 Then
                Me.ShowMessage("The file is too large to view.")
                Return
            End If

            Me.DrawViewFileFrame(fileObject.Name)
            Dim fileContent As String = Me.ReadFileToString(fileObject.FullName, Encoding.ASCII)
            Dim beginPosition As Integer = 0
            Dim symbolCount As Integer = 0
            Dim endOfFile As Boolean = False
            Dim beginFile As Boolean = True
            Dim printSymbols As Stack(Of Integer) = New Stack(Of Integer)()
            symbolCount = Me.PrintStingFrame(fileContent, beginPosition)
            printSymbols.Push(symbolCount)
            Me.PrintProgress(beginPosition + symbolCount, fileContent.Length)
            Dim [exit] As Boolean = False

            While Not [exit]
                endOfFile = (beginPosition + symbolCount) >= fileContent.Length
                beginFile = (beginPosition <= 0)
                Dim userKey As ConsoleKeyInfo = Console.ReadKey(True)

                Select Case userKey.Key
                    Case ConsoleKey.Escape
                        [exit] = True
                    Case ConsoleKey.PageDown

                        If Not endOfFile Then
                            beginPosition += symbolCount
                            symbolCount = Me.PrintStingFrame(fileContent, beginPosition)
                            printSymbols.Push(symbolCount)
                            Me.PrintProgress(beginPosition + symbolCount, fileContent.Length)
                        End If

                    Case ConsoleKey.PageUp

                        If Not beginFile Then

                            If printSymbols.Count <> 0 Then
                                beginPosition -= printSymbols.Pop()

                                If beginPosition < 0 Then
                                    beginPosition = 0
                                End If
                            Else
                                beginPosition = 0
                            End If

                            symbolCount = Me.PrintStingFrame(fileContent, beginPosition)
                            Me.PrintProgress(beginPosition + symbolCount, fileContent.Length)
                        End If
                End Select
            End While

            Console.Clear()

            For Each fp As FilePanel In panels
                fp.Show()
            Next

            Me.ShowKeys()
        End Sub
        Private Sub DrawViewFileFrame(ByVal file As String)
            Console.Clear()
            PsCon.PsCon.PrintFrameDoubleLine(0, 0, Console.WindowWidth, Console.WindowHeight - 5, ConsoleColor.DarkYellow, ConsoleColor.Black)
            Dim fileName As String = String.Format(" {0} ", file)
            PsCon.PsCon.PrintString(fileName, (Console.WindowWidth - fileName.Length) / 2, 0, ConsoleColor.Yellow, ConsoleColor.Black)
            PsCon.PsCon.PrintFrameLine(0, Console.WindowHeight - 5, Console.WindowWidth, 4, ConsoleColor.DarkYellow, ConsoleColor.Black)
            PsCon.PsCon.PrintString("PageDown / PageUp - Navigate, Esc - Save", 1, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black)
        End Sub
        Private Sub PrintProgress(ByVal position As Integer, ByVal length As Integer)
            Dim pageMessage As String = String.Format("File Progress: {0}%", (100 * position) / length)
            PsCon.PsCon.PrintString(New String(" "c, Console.WindowWidth / 2 - 1), Console.WindowWidth / 2, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black)
            PsCon.PsCon.PrintString(pageMessage, Console.WindowWidth - pageMessage.Length - 2, Console.WindowHeight - 4, ConsoleColor.White, ConsoleColor.Black)
        End Sub
        Private Function ReadFileToString(ByVal fullFileName As String, ByVal encoding As Encoding) As String
            Dim SR As StreamReader = New StreamReader(fullFileName, encoding)
            Dim fileContent As String = SR.ReadToEnd()
            fileContent = fileContent.Replace(ChrW(7), " ").Replace(vbBack, " ").Replace(vbFormFeed, " ").Replace(vbCr, " ").Replace(vbVerticalTab, " ")
            SR.Close()
            Return fileContent
        End Function
        Private Sub PrintStingFrame(ByVal text As String)
            Console.SetCursorPosition(1, 1)
            Dim frameWidth As Integer = Console.WindowWidth - 2
            Dim colCount As Integer = 0
            Dim rowCount As Integer = 1
            Dim symbolIndex As Integer = 0

            While symbolIndex < text.Length

                If colCount = frameWidth Then
                    rowCount += 1
                    Console.SetCursorPosition(1, rowCount)
                    colCount = 0
                End If

                Console.Write(text(symbolIndex))
                symbolIndex += 1
                colCount += 1
            End While
        End Sub

        Private Function PrintStingFrame(ByVal text As String, ByVal begin As Integer) As Integer
            Me.ClearFileViewFrame()
            Dim lastTopCursorPosition As Integer = Console.WindowHeight - 7
            Dim lastLeftCursorPosition As Integer = Console.WindowWidth - 2
            Console.SetCursorPosition(1, 1)
            Dim currentTopPosition As Integer = Console.CursorTop
            Dim currentLeftPosition As Integer = Console.CursorLeft
            Dim index As Integer = begin

            While True

                If index >= text.Length Then
                    Exit While
                End If

                Console.Write(text(index))
                currentTopPosition = Console.CursorTop
                currentLeftPosition = Console.CursorLeft

                If currentLeftPosition = 0 OrElse currentLeftPosition = lastLeftCursorPosition Then
                    Console.CursorLeft = 1
                End If

                If currentTopPosition = lastTopCursorPosition Then
                    Exit While
                End If

                index += 1
            End While

            Return index - begin
        End Function

        Private Sub ClearFileViewFrame()
            Dim lastTopCursorPosition As Integer = Console.WindowHeight - 7
            Dim lastLeftCursorPosition As Integer = Console.WindowWidth - 2

            For row As Integer = 1 To lastTopCursorPosition - 1
                Console.SetCursorPosition(1, row)
                Dim space As String = New String(" "c, lastLeftCursorPosition)
                Console.Write(space)
            Next
        End Sub
        Private Sub FindFile()
            If Me.panels(Me.activePanelIndex).isDiscs Then
                Return
            End If

            Dim fileName As String = Me.AksName("Enter the name: ")

            If Not Me.panels(Me.activePanelIndex).FindFile(fileName) Then
                Me.ShowMessage("File / directory in current directory not found")
            End If
        End Sub
        Private Sub RefreshPannels()
            If Me.panels Is Nothing OrElse Me.panels.Count = 0 Then
                Return
            End If

            For Each panel As FilePanel In panels

                If Not panel.isDiscs Then
                    panel.UpdateContent(True)
                End If
            Next
        End Sub

        Private Sub ChangeActivePanel()
            Me.panels(Me.activePanelIndex).Active = False
            RemoveHandler KeyPress, AddressOf Me.panels(Me.activePanelIndex).KeyboardProcessing
            Me.panels(Me.activePanelIndex).UpdateContent(False)
            Me.activePanelIndex += 1

            If Me.activePanelIndex >= Me.panels.Count Then
                Me.activePanelIndex = 0
            End If

            Me.panels(Me.activePanelIndex).Active = True
            AddHandler KeyPress, AddressOf Me.panels(Me.activePanelIndex).KeyboardProcessing
            Me.panels(Me.activePanelIndex).UpdateContent(False)
        End Sub

        Private Sub ChangeDirectoryOrRunProcess()
            Dim fsInfo As FileSystemInfo = Me.panels(Me.activePanelIndex).GetActiveObject()

            If fsInfo IsNot Nothing Then

                If TypeOf fsInfo Is DirectoryInfo Then

                    Try
                        Directory.GetDirectories(fsInfo.FullName)
                    Catch
                        Return
                    End Try

                    Me.panels(Me.activePanelIndex).Path = fsInfo.FullName
                    Me.panels(Me.activePanelIndex).SetLists()
                    Me.panels(Me.activePanelIndex).UpdatePanel()
                Else
                    Process.Start((CType(fsInfo, FileInfo)).FullName)
                End If
            Else
                Dim currentPath As String = Me.panels(Me.activePanelIndex).Path
                Dim currentDirectory As DirectoryInfo = New DirectoryInfo(currentPath)
                Dim upLevelDirectory As DirectoryInfo = currentDirectory.Parent

                If upLevelDirectory IsNot Nothing Then
                    Me.panels(Me.activePanelIndex).Path = upLevelDirectory.FullName
                    Me.panels(Me.activePanelIndex).SetLists()
                    Me.panels(Me.activePanelIndex).UpdatePanel()
                Else
                    Me.panels(Me.activePanelIndex).SetDiscs()
                    Me.panels(Me.activePanelIndex).UpdatePanel()
                End If
            End If
        End Sub

        Private Sub ShowKeys()
            Dim menu As String() = {"F3 View", " F4 Search", "F5 Copy", "F6 Move", "F7 Create", "F8 Rename", "F9 Delete", "F10 Exit"}
            Dim cellLeft As Integer = Me.panels(0).Left
            Dim cellTop As Integer = FilePanel.PANEL_HEIGHT * Me.panels.Count
            Dim cellWidth As Integer = FilePanel.PANEL_WIDTH / menu.Length
            Dim cellHeight As Integer = FileManager.HEIGHT_KEYS

            For i As Integer = 0 To menu.Length - 1
                PsCon.PsCon.PrintFrameLine(cellLeft + i * cellWidth, cellTop, cellWidth, cellHeight, ConsoleColor.White, ConsoleColor.Black)
                PsCon.PsCon.PrintString(menu(i), cellLeft + i * cellWidth + 1, cellTop + 1, ConsoleColor.White, ConsoleColor.Black)
            Next
        End Sub
        Private Sub ShowMessage(ByVal message As String)
            PsCon.PsCon.PrintString(message, 0, Console.WindowHeight - BOTTOM_OFFSET, ConsoleColor.White, ConsoleColor.Black)
        End Sub
        Private Sub ClearMessage()
            PsCon.PsCon.PrintString(New String(" "c, Console.WindowWidth), 0, Console.WindowHeight - BOTTOM_OFFSET, ConsoleColor.White, ConsoleColor.Black)
        End Sub
    End Class
    Class FilePanel
        Public Shared PANEL_HEIGHT As Integer = 18
        Public Shared PANEL_WIDTH As Integer = 120
        Private _top As Integer

        Public Property Top As Integer
            Get
                Return _top
            End Get
            Set(ByVal value As Integer)

                If 0 <= value AndAlso value <= Console.WindowHeight - FilePanel.PANEL_HEIGHT Then
                    _top = value
                Else
                    Throw New Exception(String.Format("The top coordinates ({0}) of the file panel aren't valid", value))
                End If
            End Set
        End Property

        Private _left As Integer

        Public Property Left As Integer
            Get
                Return _left
            End Get
            Set(ByVal value As Integer)

                If 0 <= value AndAlso value <= Console.WindowWidth - FilePanel.PANEL_WIDTH Then
                    _left = value
                Else
                    Throw New Exception(String.Format("The left coordinates ({0}) of the file panel aren't valid", value))
                End If
            End Set
        End Property

        Private _height As Integer = FilePanel.PANEL_HEIGHT

        Public Property Height As Integer
            Get
                Return _height
            End Get
            Set(ByVal value As Integer)

                If 0 < value AndAlso value <= Console.WindowHeight Then
                    _height = value
                Else
                    Throw New Exception(String.Format("The height ({0}) of the file panel isn't valid", value))
                End If
            End Set
        End Property

        Private _width As Integer = FilePanel.PANEL_WIDTH

        Public Property Width As Integer
            Get
                Return Me._width
            End Get
            Set(ByVal value As Integer)

                If 0 < value AndAlso value <= Console.WindowWidth Then
                    Me._width = value
                Else
                    Throw New Exception(String.Format("The width ({0}) of the file panel isn't valid", value))
                End If
            End Set
        End Property

        Private _path As String

        Public Property Path As String
            Get
                Return Me._path
            End Get
            Set(ByVal value As String)
                Dim di As DirectoryInfo = New DirectoryInfo(value)

                If di.Exists Then
                    Me._path = value
                Else
                    Throw New Exception(String.Format("Path {0} does not exist", value))
                End If
            End Set
        End Property

        Private activeObjectIndex As Integer = 0
        Private firstObjectIndex As Integer = 0
        Private displayedObjectsAmount As Integer = PANEL_HEIGHT - 2
        Private _active As Boolean

        Public Property Active As Boolean
            Get
                Return Me._active
            End Get
            Set(ByVal value As Boolean)
                Me._active = value
            End Set
        End Property

        Private discs As Boolean

        Public ReadOnly Property isDiscs As Boolean
            Get
                Return Me.discs
            End Get
        End Property

        Private fsObjects As List(Of FileSystemInfo) = New List(Of FileSystemInfo)()

        Public Sub New()
            Me.SetDiscs()
        End Sub

        Public Sub New(ByVal path As String)
            Me.path = path
            Me.SetLists()
        End Sub

        Public Function GetActiveObject() As FileSystemInfo
            If Me.fsObjects IsNot Nothing AndAlso Me.fsObjects.Count <> 0 Then
                Return Me.fsObjects(Me.activeObjectIndex)
            End If

            Throw New Exception("The list of panel objects is empty")
        End Function

        Public Function FindFile(ByVal name As String) As Boolean
            Dim index As Integer = 0

            For Each file As FileSystemInfo In Me.fsObjects

                If file IsNot Nothing AndAlso file.Name = name Then
                    Me.activeObjectIndex = index

                    If Me.activeObjectIndex > Me.displayedObjectsAmount Then
                        Me.firstObjectIndex = activeObjectIndex
                    End If

                    Me.UpdateContent(False)
                    Return True
                End If

                index += 1
            Next

            Return False
        End Function

        Public Sub KeyboardProcessing(ByVal key As ConsoleKeyInfo)
            Select Case key.Key
                Case ConsoleKey.UpArrow
                    Me.ScrollUp()
                Case ConsoleKey.DownArrow
                    Me.ScrollDown()
                Case ConsoleKey.Home
                    Me.GoBegin()
                Case ConsoleKey.[End]
                    Me.GoEnd()
                Case ConsoleKey.PageUp
                    Me.PageUp()
                Case ConsoleKey.PageDown
                    Me.PageDown()
                Case Else
            End Select
        End Sub

        Private Sub ScrollDown()
            If Me.activeObjectIndex >= Me.firstObjectIndex + Me.displayedObjectsAmount - 1 Then
                Me.firstObjectIndex += 1

                If Me.firstObjectIndex + Me.displayedObjectsAmount >= Me.fsObjects.Count Then
                    Me.firstObjectIndex = Me.fsObjects.Count - Me.displayedObjectsAmount
                End If

                Me.activeObjectIndex = Me.firstObjectIndex + Me.displayedObjectsAmount - 1
                Me.UpdateContent(False)
            Else

                If Me.activeObjectIndex >= Me.fsObjects.Count - 1 Then
                    Return
                End If

                Me.DeactivateObject(Me.activeObjectIndex)
                Me.activeObjectIndex += 1
                Me.ActivateObject(Me.activeObjectIndex)
            End If
        End Sub

        Private Sub ScrollUp()
            If Me.activeObjectIndex <= Me.firstObjectIndex Then
                Me.firstObjectIndex -= 1

                If Me.firstObjectIndex < 0 Then
                    Me.firstObjectIndex = 0
                End If

                Me.activeObjectIndex = firstObjectIndex
                Me.UpdateContent(False)
            Else
                Me.DeactivateObject(Me.activeObjectIndex)
                Me.activeObjectIndex -= 1
                Me.ActivateObject(Me.activeObjectIndex)
            End If
        End Sub

        Private Sub GoEnd()
            If Me.firstObjectIndex + Me.displayedObjectsAmount < Me.fsObjects.Count Then
                Me.firstObjectIndex = Me.fsObjects.Count - Me.displayedObjectsAmount
            End If

            Me.activeObjectIndex = Me.fsObjects.Count - 1
            Me.UpdateContent(False)
        End Sub

        Private Sub GoBegin()
            Me.firstObjectIndex = 0
            Me.activeObjectIndex = 0
            Me.UpdateContent(False)
        End Sub

        Private Sub PageDown()
            If Me.activeObjectIndex + Me.displayedObjectsAmount < Me.fsObjects.Count Then
                Me.firstObjectIndex += Me.displayedObjectsAmount
                Me.activeObjectIndex += Me.displayedObjectsAmount
            Else
                Me.activeObjectIndex = Me.fsObjects.Count - 1
            End If

            Me.UpdateContent(False)
        End Sub

        Private Sub PageUp()
            If Me.activeObjectIndex > Me.displayedObjectsAmount Then
                Me.firstObjectIndex -= Me.displayedObjectsAmount

                If Me.firstObjectIndex < 0 Then
                    Me.firstObjectIndex = 0
                End If

                Me.activeObjectIndex -= Me.displayedObjectsAmount

                If Me.activeObjectIndex < 0 Then
                    Me.activeObjectIndex = 0
                End If
            Else
                Me.firstObjectIndex = 0
                Me.activeObjectIndex = 0
            End If

            Me.UpdateContent(False)
        End Sub

        Public Sub SetLists()
            If Me.fsObjects.Count <> 0 Then
                Me.fsObjects.Clear()
            End If

            Me.discs = False
            Dim levelUpDirectory As DirectoryInfo = Nothing
            Me.fsObjects.Add(levelUpDirectory)
            Dim directories As String() = Directory.GetDirectories(Me.path)

            For Each directory As String In directories
                Dim di As DirectoryInfo = New DirectoryInfo(directory)
                Me.fsObjects.Add(di)
            Next

            Dim files As String() = Directory.GetFiles(Me.path)

            For Each file As String In files
                Dim fi As FileInfo = New FileInfo(file)
                Me.fsObjects.Add(fi)
            Next
        End Sub

        Public Sub SetDiscs()
            If Me.fsObjects.Count <> 0 Then
                Me.fsObjects.Clear()
            End If

            Me.discs = True
            Dim discs As DriveInfo() = DriveInfo.GetDrives()

            For Each disc As DriveInfo In discs

                If disc.IsReady Then
                    Dim di As DirectoryInfo = New DirectoryInfo(disc.Name)
                    Me.fsObjects.Add(di)
                End If
            Next
        End Sub

        Public Sub Show()
            Me.Clear()
            PsCon.PsCon.PrintFrameDoubleLine(Me.left, Me.top, Me.width, Me.height, ConsoleColor.White, ConsoleColor.Black)
            Dim caption As StringBuilder = New StringBuilder()

            If Me.discs Then
                caption.Append(" "c).Append("Discs").Append(" "c)
            Else
                caption.Append(" "c).Append(Me.path).Append(" "c)
            End If

            PsCon.PsCon.PrintString(caption.ToString(), Me.left + Me.width / 2 - caption.ToString().Length / 2, Me.top, ConsoleColor.White, ConsoleColor.Black)
            Me.PrintContent()
        End Sub

        Public Sub Clear()
            For i As Integer = 0 To Me.height - 1
                Dim space As String = New String(" "c, Me.width)
                Console.SetCursorPosition(Me.left, Me.top + i)
                Console.Write(space)
            Next
        End Sub

        Private Sub PrintContent()
            If Me.fsObjects.Count = 0 Then
                Return
            End If

            Dim count As Integer = 0
            Dim lastElement As Integer = Me.firstObjectIndex + Me.displayedObjectsAmount

            If lastElement > Me.fsObjects.Count Then
                lastElement = Me.fsObjects.Count
            End If

            If Me.activeObjectIndex >= Me.fsObjects.Count Then
                activeObjectIndex = 0
            End If

            For i As Integer = Me.firstObjectIndex To lastElement - 1
                Console.SetCursorPosition(Me.left + 1, Me.top + count + 1)

                If i = Me.activeObjectIndex AndAlso Me.active = True Then
                    Console.ForegroundColor = ConsoleColor.Black
                    Console.BackgroundColor = ConsoleColor.White
                End If

                Me.PrintObject(i)
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Black
                count += 1
            Next
        End Sub

        Private Sub ClearContent()
            For i As Integer = 1 To Me.height - 1 - 1
                Dim space As String = New String(" "c, Me.width - 2)
                Console.SetCursorPosition(Me.left + 1, Me.top + i)
                Console.Write(space)
            Next
        End Sub

        Private Sub PrintObject(ByVal index As Integer)
            If index < 0 OrElse Me.fsObjects.Count <= index Then
                Throw New Exception(String.Format("Unable to output object with index {0}. Index out of range", index))
            End If

            Dim currentCursorTopPosition As Integer = Console.CursorTop
            Dim currentCursorLeftPosition As Integer = Console.CursorLeft

            If Not Me.discs AndAlso index = 0 Then
                Console.Write("..")
                Return
            End If

            Console.Write("{0}", fsObjects(index).Name)
            Console.SetCursorPosition(currentCursorLeftPosition + Me.width / 2, currentCursorTopPosition)

            If TypeOf fsObjects(index) Is DirectoryInfo Then
                Console.Write("{0}", (CType(fsObjects(index), DirectoryInfo)).LastWriteTime)
            Else
                Console.Write("{0}", (CType(fsObjects(index), FileInfo)).Length)
            End If
        End Sub

        Public Sub UpdatePanel()
            Me.firstObjectIndex = 0
            Me.activeObjectIndex = 0
            Me.Show()
        End Sub

        Public Sub UpdateContent(ByVal updateList As Boolean)
            If updateList Then
                Me.SetLists()
            End If

            Me.ClearContent()
            Me.PrintContent()
        End Sub

        Private Sub ActivateObject(ByVal index As Integer)
            Dim offsetY As Integer = Me.activeObjectIndex - Me.firstObjectIndex
            Console.SetCursorPosition(Me.left + 1, Me.top + offsetY + 1)
            Console.ForegroundColor = ConsoleColor.Black
            Console.BackgroundColor = ConsoleColor.White
            Me.PrintObject(index)
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
        End Sub

        Private Sub DeactivateObject(ByVal index As Integer)
            Dim offsetY As Integer = Me.activeObjectIndex - Me.firstObjectIndex
            Console.SetCursorPosition(Me.left + 1, Me.top + offsetY + 1)
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            Me.PrintObject(index)
        End Sub
    End Class
End Namespace
Namespace SoundcloudDownloadFramwork
    Class Program
        Shared clientID As String = "b45b1aa10f1ac2941910a7f0d10f8e28"
        Public Structure TrackID
            Public index As Integer
            Public id As String
            Public title As String
            Public Sub New(ByVal i As Integer, ByVal j As String, ByVal k As String)
                index = i
                id = j
                title = k
            End Sub
        End Structure
        Shared Sub Main(ByVal args As String())
                Console.Write("Please enter the URL of the playlist/song to download: ")
                Dim url As String = Console.ReadLine()
                If url.Contains("/sets/") Then
                    Console.WriteLine("Downloading playlist.")
                    downloadSet(url)
                Else
                    downloadSong(url)
                End If
            End Sub
        Public Shared Function resolveTrackID(ByVal url As String, ByVal startindex As Integer)
            Dim w As WebClient = New WebClient()
            Dim str As String = w.DownloadString("http://api.soundcloud.com/resolve.json?url=" + url + "&client_id=" + clientID)
            Dim index As Integer = str.IndexOf("""track"",""id"":", startindex) + 13
            Dim titleindex As Integer = str.IndexOf("""title"":", index) + 9
            Return New TrackID(index, str.Substring(index, str.IndexOf(",", index) - index), str.Substring(titleindex, str.IndexOf(""",", titleindex) - titleindex))
        End Function
        Public Shared Function resolveTrackIDs(ByVal url As String) As List(Of TrackID)
            Dim ret As List(Of TrackID) = New List(Of TrackID)()
            Dim currentindex As Integer = 0
            For i As Integer = 0 To getTrackCount(url) - 1
                Dim t As TrackID = resolveTrackID(url, currentindex)
                currentindex = t.index + 1
                ret.Add(t)
            Next
            Return ret
        End Function
        Public Shared Function getTrackCount(ByVal url As String) As Integer
            Dim w As WebClient = New WebClient()
            Dim str As String = w.DownloadString("http://api.soundcloud.com/resolve.json?url=" & url & "&client_id=" + clientID)
            Dim index As Integer = str.IndexOf("""track_count"":") + 14
            Return Convert.ToInt32(str.Substring(index, str.IndexOf(",", index) - index))
        End Function
        Public Shared Function resolveDownloadURL(ByVal trackID As String) As String
            Dim w As WebClient = New WebClient()
            Dim str As String = w.DownloadString("https://api.soundcloud.com/tracks/" & trackID & "/streams?client_id=" + clientID)
            Dim index As Integer = str.IndexOf(":""http") + 2
            Return str.Substring(index, str.IndexOf("""", index) - index)
        End Function
        Public Shared Sub downloadSong(ByVal url As String)
            Dim trackID As TrackID = resolveTrackID(url, 0)
            Dim downloadurl As String = resolveDownloadURL(trackID.id).Replace("\u0026", "&")
            Dim w As WebClient = New WebClient()
            Dim filename As String = trackID.title + ".mp3"
            If filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then
                For Each c As Char In New String(Path.GetInvalidFileNameChars())
                    filename = filename.Replace(c.ToString(), "")
                Next
            End If
            Console.WriteLine("Downloading " + filename + ".")
            w.DownloadFile(downloadurl, filename)
            Console.WriteLine("Finished downloading " + filename + ".")
        End Sub
        Public Shared Sub downloadSet(ByVal url As String)
            Dim trackIDs As List(Of TrackID) = New List(Of TrackID)(resolveTrackIDs(url))
            For Each trackID As TrackID In trackIDs
                Dim downloadurl As String = Regex.Unescape(resolveDownloadURL(trackID.id))
                Dim w As WebClient = New WebClient()
                Dim filename As String = Regex.Unescape(trackID.title) & ".mp3"

                If filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 Then

                    For Each c As Char In New String(Path.GetInvalidFileNameChars())
                        filename = filename.Replace(c.ToString(), "")
                    Next
                End If

                Console.WriteLine("Downloading " & filename & ".")
                w.DownloadFile(downloadurl, filename)
                Console.WriteLine("Finished downloading " & filename & ".")
            Next

            Console.WriteLine("Finished downloading the playlist.")
        End Sub
    End Class
End Namespace

