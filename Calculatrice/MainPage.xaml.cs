using System.Globalization;

namespace Calculatrice;

public partial class MainPage : ContentPage
{
    // Etat de la calculatrice
    string currentInput = "0";       
    double? firstOperand = null;     
    string? selectedOperator = null; 
    bool isNewInput = true;          
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

    // Exécution du calcul 
    static double Calculate(double a, double b, string op)
    {
        return op switch
        {
            "+" => a + b,
            "−" => a - b,
            "×" => a * b,
            "÷" => a / b,
            "%" => a % b,
            _ => b
        };
    }

    // Permet la mise à jour des deux zones d'affichage liées à l'état
    void UpdateDisplay()
    {
        ResultLabel.Text = currentInput;
        OperatorLabel.Text = selectedOperator ?? "—";
    }

    // Remise à zero 
    void ResetAll()
    {
        currentInput = "0";
        firstOperand = null;
        selectedOperator = null;
        isNewInput = true;
        OperationLabel.Text = "";
        UpdateDisplay();
    }

    // Evènements liés aux touches du clavier
    // Touches de 0 à 9
    void OnDigitClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;

        string digit = button.Text;

        if (isNewInput)
        {
            if (selectedOperator == null) OperationLabel.Text = "";
            currentInput = digit;
            isNewInput = false;
        }
        else
        {
            currentInput = currentInput == "0" ? digit : currentInput + digit;
        }

        UpdateDisplay();
    }

    // Touche .
    void OnDecimalClicked(object? sender, EventArgs e)
    {
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
        if (sender is not Button button) return;

        string op = button.Text;

        // Si deux operateurs de suite sont cliqués alors on remplace le precedent
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
            current = Calculate(firstOperand.Value, current, selectedOperator);
            currentInput = Format(current);
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
        // Si rien à calculer (pas d'opération, ou pas de deuxième nombre)
        if (selectedOperator == null || firstOperand == null || isNewInput) return;

        double second = Parse(currentInput);
        double result = Calculate(firstOperand.Value, second, selectedOperator);

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

    // Touche ⌫
    void OnBackspaceClicked(object? sender, EventArgs e)
    {
        // On n'efface pas un resultat ni un nombre memorise 
        if (isNewInput) return;

        currentInput = currentInput[..^1];
        if (currentInput is "" or "-" or "-0") currentInput = "0";

        UpdateDisplay();
    }

    // Touche ±
    void OnSignClicked(object? sender, EventArgs e)
    {
        if (currentInput == "0") return;

        currentInput = currentInput.StartsWith('-') ? currentInput[1..] : "-" + currentInput;

        // Si une operation est en attente le nombre affiché devient le deuxième nombre
        if (selectedOperator != null) isNewInput = false;

        UpdateDisplay();
    }

}