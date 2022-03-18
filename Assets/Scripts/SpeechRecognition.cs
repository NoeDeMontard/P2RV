using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class SpeechRecognition : MonoBehaviour
{
    public GameObject move_go;
    public GameObject grab_go;
    public GameObject sphere_go;
    /// <summary>
    /// Class to store the method associated to a command, as well as it's parameters.
    /// </summary>
    public class Command
    {
        public enum Type
        {
            None,
            selection,
            deplacement
        }

        #region Members
        public Type mode;
        #endregion

        #region Constructors
        // Construct a new command object.
        public Command()
        {
            mode = Type.None;
        }
        #endregion

        #region Overloads
        /// <summary>
        /// Convert the command into a string representation.
        /// </summary>
        /// <returns>The string representing the command values.</returns>
        public override string ToString()
        {
            return string.Format("{0}", mode);
        }
        #endregion
    }

    #region Members
    private GrammarRecognizer recognizer;
    private IReadOnlyDictionary<string, Command.Type> dict;

    #endregion

    #region MonoBehaviour callbacks

    private void Awake()
    {
       
        // Create a dictionary to simplify the linking between specific words and wanted mode
        dict = new Dictionary<string, Command.Type>{
            { "déplacement", Command.Type.deplacement},
            { "déplace", Command.Type.deplacement},
            { "déplacer", Command.Type.deplacement},
            { "bouger", Command.Type.deplacement},
            { "bouge", Command.Type.deplacement},
            { "sélection", Command.Type.selection},
            { "sélectionner", Command.Type.selection},
            { "sélectionne", Command.Type.selection},
            { "attraper", Command.Type.selection},
            { "attrape", Command.Type.selection}
        };

		
		recognizer = new GrammarRecognizer(Application.dataPath + "/Grammar.xml");
        recognizer.OnPhraseRecognized += OnRecognition;
        recognizer.Start();

        // OK, we are ready to start.
        Debug.Log("Speach recognition ready");
    }

    private void OnDestroy()
    {
        // Close the external speech recognition program on exit.
        if(recognizer != null)
        {
            if(recognizer.IsRunning)
            {
                recognizer.Stop();
            }

            // Close process by sending a close message to its main window.
            recognizer.Dispose();
        }
    }

    private void Update(){}
    #endregion

    #region Internal methods
	/// <summary>
    /// Process a recognized sentence.
    /// </summary>
    /// <param name="args">The parameters of the recognized sentence.</param>
    private void OnRecognition(PhraseRecognizedEventArgs args)
    {
		Debug.LogFormat("{0} - {1}", args.text, args.confidence);
        if (args.confidence == ConfidenceLevel.High || args.confidence == ConfidenceLevel.Medium)
        {
			Command cmd = new Command();

            String phrase = args.text;
            String[] words = phrase.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                HandleRecognition(cmd, words[i]);
            }

            ExecCommand(cmd);

            // Update debug display.
            Debug.Log(cmd.ToString());
        }
    }

    /// <summary>
    /// Process the current word to update the command parameters.
    /// </summary>
    /// <param name="cmd">The command to update.</param>
    /// <param name="word">The word to analyse.</param>
    private void HandleRecognition(Command cmd, string word)
    {
        // Make sure the word is in lower case, to avoid problems of comparing the same word using different cases.
        string w = word.ToLowerInvariant();

        // Check if the word is contained into the matching dictionary.
        if(dict.TryGetValue(w, out Command.Type a))
        {
            cmd.mode = a;
        }
    }

    /// <summary>
    /// Execute a command once it's completed.
    /// </summary>
    /// <param name="cmd">Command to execute.</param>
    private void ExecCommand(Command cmd)
    {
        switch (cmd.mode)
        {
            case Command.Type.deplacement:
                grab_go.GetComponent<globalGrab>().enabled = false;
                move_go.GetComponent<gazeMove>().enabled = true;
                sphere_go.SetActive(false); // TODO : tester
                break;
            case Command.Type.selection:
                grab_go.GetComponent<globalGrab>().enabled = true;
                move_go.GetComponent<gazeMove>().enabled = false;
                sphere_go.SetActive(true); // TODO : tester
                break;
        }
	}
    #endregion
}
