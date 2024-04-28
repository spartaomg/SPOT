Friend Module ModHungarian

    Const rows As Integer = 16
    Const cols As Integer = 16

    Private NumCovered As Integer = 0

    Private matrix(rows - 1, cols - 1) As Double
    Private amatrix(rows - 1, cols - 1) As Integer
    Private pmatrix(rows - 1, cols - 1) As Integer

    Private CoveredRows(rows - 1) As Integer
    Private CoveredCols(cols - 1) As Integer

    Public PaletteAssignment(15) As Integer

    Public Function HungarianAlgorith(m(,) As Double) As Double

        ReDim amatrix(rows - 1, cols - 1), pmatrix(rows - 1, cols - 1)
        ReDim CoveredRows(rows - 1), CoveredCols(cols - 1)
        ReDim PaletteAssignment(15)
        NumCovered = 0

        'Create copy of m matrix and reset amatrix And pmatrix
        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                matrix(r, c) = m(r, c)
            Next
        Next

        UncoverLines()                'Also resets Numcovered, And clears amatrix And pmatirx

        FindOptimalAssignment()

        Dim cost As Double = 0.0

        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                If amatrix(r, c) = 1 Then
                    PaletteAssignment(c) = r
                    cost += m(r, c)
                End If
            Next
        Next

        Return cost

    End Function

    Private Sub FindOptimalAssignment()
        'INITIAL STEPS

        'For each row in the matrix, find the smallest element And subtract it from every element in the row.
        'If there Is at least one 0 in each column then we are done

        For r As Integer = 0 To rows - 1

            Dim MinVal As Double = matrix(r, 0)

            For c As Integer = 1 To cols - 1
                MinVal = Math.Min(MinVal, matrix(r, c))
            Next

            For c As Integer = 0 To cols - 1
                matrix(r, c) -= MinVal
                If matrix(r, c) = 0 Then
                    If CoveredCols(c) = 0 Then
                        CoveredCols(c) = 1      'Cover column
                        NumCovered += 1         'Also count covered columns
                    End If
                End If
            Next
        Next

        If NumCovered = cols Then               'If all columns are covere then we are done
            AssignZerosAndCoverColumns()
            Exit Sub
        End If

        'Subtract the smallest value in each column

        For c As Integer = 0 To cols - 1
            If CoveredCols(c) = 0 Then

                Dim MinVal As Double = matrix(0, c)

                For r As Integer = 1 To rows - 1
                    MinVal = Math.Min(matrix(r, c), MinVal)
                Next

                For r As Integer = 0 To rows - 1
                    matrix(r, c) -= MinVal
                Next
            Else
                CoveredCols(c) = 0              'Otherwise uncover column
            End If
        Next

        AssignZerosAndCoverColumns()

        CoverLines()

    End Sub

    Private Sub CoverLines()

        While NumCovered < cols

            Dim Found0 As Boolean = True
            Dim Flipped0 As Boolean = False

            While (Found0 = True) And (Flipped0 = False)
                Found0 = False
                For c As Integer = 0 To cols - 1
                    If CoveredCols(c) = 0 Then
                        For r As Integer = 0 To rows - 1
                            If CoveredRows(r) = 0 Then
                                If matrix(r, c) = 0 Then
                                    pmatrix(r, c) = 1          'Uncovered 0 found, prime it

                                    'Primed 0s are either in the same row or the same col of an assigned 0
                                    Dim ac As Integer
                                    For ac = 0 To cols - 1
                                        If amatrix(r, ac) = 1 Then
                                            Exit For
                                        End If
                                    Next
                                    If ac < cols Then
                                        'this row does have an assigned 0, in col AssignedCol[r]
                                        'uncover the assigned 0's column and cover this row
                                        'then continue looking for an uncovered 0 until no more found
                                        CoveredRows(r) = 1
                                        CoveredCols(ac) = 0
                                        Found0 = True
                                        Exit For
                                    Else
                                        'this row does Not have an assigned 0
                                        'so this column must have one
                                        FlipZeros(r, c)
                                        '-> flip primed And assigned 0s
                                        '-> update amatrix, delete pmatrix, uncover all rows, cover assigned columens)
                                        Flipped0 = True
                                        Exit For
                                        'We will exit both [c] and [r] for loops and the ((Found0) && (!Flipped0) while loop too

                                    End If
                                End If
                            End If
                        Next
                    End If
                    'We exit both for loops if (Flipped0), but only the inner loop if (Found0)
                    If Flipped0 = True Then
                        Exit For
                    End If
                Next
            End While

            If Flipped0 = False Then
                'we ran out of unassinged 0's (without flipping, i.e. we didn't change the number of 0s and covered lines)
                'we need to create more 0s
                CreateNewZeros()
                '->create more 0s
                '->uncover all lines, clear amatrix And pmatrix
                '->assign 0s And cover columns, calculate NumCovered
            End If
        End While

    End Sub

    Private Sub FlipZeros(pr As Integer, pc As Integer)
        Dim ac As Integer = pc                      'the original assigned 0 Is On the same column
        Dim ar As Integer

        For ar = 0 To rows - 1                      'finds its row
            If amatrix(ar, ac) = 1 Then
                Exit For
            End If
        Next

        amatrix(pr, pc) = 1                         'assign primed 0

        While ar < rows
            'find primed 0 - there always must be one

            pr = ar                                 'same row as the original assigned 0 (now un-assigned)

            For pc = 0 To cols - 1
                If pmatrix(pr, pc) = 1 Then
                    Exit For
                End If
            Next

            amatrix(ar, ac) = 0                     'unassign assigned 0

            'find assigned 0 in current column

            ac = pc
            For ar = 0 To rows - 1
                If amatrix(ar, ac) = 1 Then
                    Exit For
                End If
            Next

            'assign primed 0

            amatrix(pr, pc) = 1                         'assign this 0

        End While

        'Update amatrix, delete pmatrix, uncover all rows, cover assigned columens

        NumCovered = 0

        For ar = 0 To rows - 1
            CoveredRows(ar) = 0
            For ac = 0 To cols - 1
                If amatrix(ar, ac) = 1 Then
                    CoveredCols(ac) = 1
                    NumCovered += 1
                End If
                pmatrix(ar, ac) = 0
            Next
        Next

    End Sub

    Private Sub CreateNewZeros()
        'Find smallest uncovered element in matrix
        'Subtract it from all uncovered elements
        'Add it to all double-covered elements

        Dim MinVal As Double = Double.MaxValue

        For r As Integer = 0 To rows - 1
            If CoveredRows(r) = 0 Then
                For c As Integer = 0 To cols - 1
                    If CoveredCols(c) = 0 Then
                        MinVal = Math.Min(matrix(r, c), MinVal)
                    End If
                Next
            End If
        Next

        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                If (CoveredRows(r) = 0) And (CoveredCols(c) = 0) Then
                    matrix(r, c) -= MinVal
                ElseIf (CoveredRows(r) = 1) And (CoveredCols(c) = 1) Then
                    matrix(r, c) += MinVal
                End If
            Next
        Next

        'Uncover lines, create assigned matrix, cover columns, calculate NumCovered, create assigned indices

        AssignZerosAndCoverColumns()

    End Sub

    Private Sub AssignZerosAndCoverColumns()
        'Assign the first 0s in each row that are Not in a covered column
        'Save assigned indices
        'Calculate NumCovered

        UncoverLines()

        For r As Integer = 0 To rows - 1
            For c As Integer = 0 To cols - 1
                If CoveredCols(c) = 0 Then
                    If matrix(r, c) = 0 Then
                        NumCovered += 1         'Count covered lines

                        CoveredCols(c) = 1      'Build CoveredCols array
                        'CoveredRows(r) = 1
                        amatrix(r, c) = 1       'Build assigned matrix
                        Exit For
                    End If
                End If
            Next
        Next

        'For r As Integer = 0 To rows - 1
        'CoveredRows(r) = 0
        'Next

    End Sub

    Private Sub UncoverLines()

        ReDim amatrix(rows - 1, cols - 1), pmatrix(rows - 1, cols - 1)

        For r As Integer = 0 To rows - 1

            CoveredRows(r) = 0

            'For c As Integer = 0 To cols - 1
            'amatrix(r, c) = 0
            'pmatrix(r, c) = 0
            'Next

        Next

        For c As Integer = 0 To cols - 1
            CoveredCols(c) = 0
        Next

        NumCovered = 0

    End Sub

End Module
