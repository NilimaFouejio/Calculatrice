using System.Globalization;

namespace Calculatrice;

public partial class MainPage : ContentPage
{
    // Nombre maximum de chiffres saisissables 
    const int MaxDigits = 12;

    // Etat de la calculatrice 
    string currentInput = "0";       
    double? firstOperand = null;     
    string? selectedOperator = null; 
    bool isNewInput = true;          
    bool hasError = false;           

    public MainPage()
    {
        InitializeComponent();
    }


    // Transforme un nombre en texte
    static string Format(double value)
    {
        if (value == 0) value = 0;
        return value.ToString("G12", CultureInfo.InvariantCulture);
    }

    // Transforme le texte affiché en nombre
    static double Parse(string text)
    {
        return double.Parse(text, CultureInfo.InvariantCulture);
    }

    // Effectue le calcul et renvoie false si le calcul est impossible
    static bool TryCalculate(double a, double b, string op, out double result, out string error)
    {
        result = 0;
        error = "";

        if (op == "÷" && b == 0)
        {
            error = "Division par zéro impossible";
            return false;
        }

        if (op == "%" && b == 0)
        {
            error = "Modulo par zéro impossible";
            return false;
        }

        result = op switch
        {
            "+" => a + b,
            "−" => a - b,
            "×" => a * b,
            "÷" => a / b,
            "%" => a % b,
            _ => b
        };

        if (double.IsInfinity(result) || double.IsNaN(result))
        {
            error = "Résultat trop grand";
            return false;
        }

        return true;
    }

    // Permet de mettre à jour les deux zones d'affichage liées a l'état
    void UpdateDisplay()
    {
        ResultLabel.Text = currentInput;
        OperatorLabel.Text = selectedOperator ?? "—";
    }

    // Remise a zero
    void ResetAll()
    {
        currentInput = "0";
        firstOperand = null;
        selectedOperator = null;
        isNewInput = true;
        hasError = false;
        OperationLabel.Text = "";
        UpdateDisplay();
    }

    // Permet l'affichage d'erreur sans faire planter la calculatrice
    void ShowError(string message)
    {
        currentInput = "0";
        firstOperand = null;
        selectedOperator = null;
        isNewInput = true;
        hasError = true;
        ResultLabel.Text = "Erreur";
        OperationLabel.Text = message;
        OperatorLabel.Text = "—";
    }

    // Evènements liés aux touches de la calculatrice

    // Touches de 0 à 9
    void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;
        if (hasError) ResetAll();

        string digit = button.Text;

        if (isNewInput)
        {
            if (selectedOperator == null) OperationLabel.Text = "";
            currentInput = digit;
            isNewInput = false;
        }
        else
        {
            int digitCount = currentInput.Count(char.IsDigit);
            if (digitCount >= MaxDigits) return;

            currentInput = currentInput == "0" ? digit : currentInput + digit;
        }

        UpdateDisplay();
    }

    // Touche .
    void OnDecimalClicked(object? sender, EventArgs e)
    {
        if (hasError) ResetAll();

        if (isNewInput)
        {
            if (selectedOperator == null) OperationLabel.Text = "";
            currentInput = "0.";
            isNewInput = false;
        }
        else if (!currentInput.Contains('.'))
        {
            currentInput += ".";
        }

        UpdateDisplay();
    }

    // Touches + − × ÷ % 
    void OnOperatorClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || hasError) return;

        string op = button.Text;

        // Si l'on clique sur deux operateurs de suite alors on remplace le précedent
        if (selectedOperator != null && isNewInput)
        {
            selectedOperator = op;
            OperationLabel.Text = $"{Format(firstOperand ?? 0)} {op}";
            UpdateDisplay();
            return;
        }

        double current = Parse(currentInput);

        // Enchainement (2 + 3 + ...) : on calcule d'abord le resultat en cours
        if (selectedOperator != null && firstOperand != null)
        {
            if (!TryCalculate(firstOperand.Value, current, selectedOperator, out double result, out string error))
            {
                ShowError(error);
                return;
            }

            current = result;
            currentInput = Format(result);
        }

        firstOperand = current;
        selectedOperator = op;
        isNewInput = true;
        OperationLabel.Text = $"{Format(current)} {op}";
        UpdateDisplay();
    }

    // Touche =
    void OnEqualsClicked(object? sender, EventArgs e)
    {
        // Si rien a calculer c'est à dire pas d'opération, ou pas de deuxième nombre 
        if (hasError || selectedOperator == null || firstOperand == null || isNewInput) return;

        double second = Parse(currentInput);

        if (!TryCalculate(firstOperand.Value, second, selectedOperator, out double result, out string error))
        {
            ShowError(error);
            return;
        }

        OperationLabel.Text = $"{Format(firstOperand.Value)} {selectedOperator} {Format(second)} =";
        currentInput = Format(result);
        firstOperand = null;
        selectedOperator = null;
        isNewInput = true;
        UpdateDisplay();
    }

    // Touche AC
    void OnClearClicked(object? sender, EventArgs e)
    {
        ResetAll();
    }

    // Touche *
    void OnBackspaceClicked(object? sender, EventArgs e)
    {
        if (hasError)
        {
            ResetAll();
            return;
        }

        // On n'efface pas un resultat ni un nombre memorise 
        if (isNewInput) return;

        currentInput = currentInput[..^1];
        if (currentInput is "" or "-" or "-0") currentInput = "0";

        UpdateDisplay();
    }

    // Touche ±
    void OnSignClicked(object? sender, EventArgs e)
    {
        if (hasError || currentInput == "0") return;

        currentInput = currentInput.StartsWith('-') ? currentInput[1..] : "-" + currentInput;

        // Si une opération est en attente, le nombre affiché devient le deuxième nombre
        if (selectedOperator != null) isNewInput = false;

        UpdateDisplay();
    }

}