using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Parser_Test
{
    //class to hold info on each interaction
    public class node
    {
        //default constructor for node
        public node()
        {
            line = null;
            response = false;
            end = false;
            owner = "default";
            sceneNum = 0;
            lineNum = 0;
            jumpNum = 0;
            reactions = new List<reply>();
        }

        //spoken dialogue
        public string line;
        //flag for whether player can respond
        public bool response;
        //flag for whether this ends the conversation
        public bool end;
        // owner of the node, is an actor this should be converted
        // to hold an actor class not a string
        public string owner;
        // id for this node
        public int sceneNum;
        // id for each resopnse in the node
        public int lineNum;
        // number of the node that a response links to if executed
        public int jumpNum;
        //list of responses
        public List<reply> reactions;

    }

    // class to hold all the responses for a node
    // inherits from node
    public class reply : node
    {
        // list of all the gates that affect this reply
        // i.e. this option won't be presented unless x has been fulfilled
        public List<gate> conditions;

        //list of effects that this will have if this reply is selected
        //to be added once a class is generated

        public reply()
        {
            owner = "Player";

        }
    }


    //structure for conversation flags
    public struct flag
    {
        //data
        public string name;
        public bool active;

        //constructor
        public flag(string name, bool active)
        {
            this.name = name;
            this.active = active;
        }
    }

    //structure that holds conditions for wheather a reply can be selected
    public class gate
    {
        //tbd
        //likely will be some sort of check 'x' to see if it meets this requirement
        //i.e. if intelligence > 12 display this reply
        //or if flag for previous diaglogue is set display reply
    }

    //structure that holds the results that will occur once a reply is selected
    public struct effect
    {
        //tbd
        //likely will be alter x by y amount
        //will also include setting of flags
        //this struct may need to be rewritten and a class
        //not sure if functionality can be implemented without a function
    }

    //holds a bunch of windows which represent nodes each of which is a line of dialogue
    //all nodes are grouped into levels
    public class tree
    {

        //default constructor
        public tree()
        {
            title = "undefined";
            actors = new List<string>();
            flags = new List<flag>();
            nodes = new List<node>();
        }

        //title of conversation
        public string title;
        //list of all owners of dialogue in the conversation
        public List<string> actors;
        
        //list of all nodes that make up the tree
        public List<node> nodes;

        //variables for conversation, flags for dialogue progression, etc
        // similar to conditions but these are booleans for line progress not for checking values 
        // probably needs encapuslated in a struct or class or merged with gates
        public List<flag> flags;

        //currently selected node
        //used for creation and navigation
        public int pc;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string tmp = File.ReadAllText("../../script.txt");
            //parse(tmp);
            transcribe(parse(tmp), "test", "../../");
        }

        //create a dialogue tree based on  text script
        static tree parse(string f)
        {
            
            //each set of lines is divided by ~
            //string[] sections = f.Split('~');
            string[] lines;
            string[] segments;
            string characters;

            //counting all the lines without data
            int offset = 0;
            int curScene = 0;

            //tree to return
            tree result = new tree();

            //the first set will always be the name of the conversation and some variable dec

            //split on ~
            string[] scenes = f.Split('~');

            for (int i = 0; i < scenes.Length; i++)
            {

                //split the section into individual lines
                lines = scenes[i].Split('\n', '\r');

                //reset the counter
                offset = 0;

                for (int j = 0; j < lines.Length; j++)
                {

                    if (lines[j] == "")
                    {
                        //offset++;
                        continue;
                    }
                    offset++;
                    //split the line into segments based on tabs
                    segments = lines[j].Split('\t');

                    for (int k = 0; k < segments.Length; k++)
                    {
                        characters = segments[k];

                        int end;
                        string tmp;

                        //checks the first part of every segment for a tag 
                        if (characters != "")
                        {
                            switch (characters[0])
                            {
                                case '/': // lines that start with // are notes and should be ignored
                                    //decrement offset to keep stable
                                    offset--;
                                    continue;

                                case '{': // find the scene num
                                    end = characters.LastIndexOf('}');
                                    tmp = characters.Substring(1, end - 1);
                                    curScene = int.Parse(tmp);
                                    //this is the only tag that will exist after the variable dec
                                    //dec offset to keep in sync
                                    offset--;
                                    break;

                                case '#': // lines starting text inside  a set of # means the title for the script
                                    end = characters.LastIndexOf('#');
                                    tmp = characters.Substring(1, end - 1);
                                    result.title = tmp;
                                    continue;

                                case '|': // | is used to declare script participants

                                    end = characters.LastIndexOf('|');

                                    tmp = characters.Substring(1, end - 1);
                                    result.actors.Add(tmp);
                                    break;

                                case '<': // <> used to declare script flags
                                    
                                    end = characters.LastIndexOf('>');
                                    tmp = characters.Substring(1, end - 1);
                                    flag tmpflag = new flag(tmp, false);
                                    result.flags.Add(tmpflag);
                                    continue;

                                case '[': // used to declare actions that should happen if this line is selected
                                    end = characters.LastIndexOf(']');
                                    tmp = characters.Substring(1, end - 1);
                                    continue;

                                default: // if none of these cases that means that no declarations are present
                                    break;
                            }

                            //after checking for tags we then parse
                            //loop through all available conversation participants to see who  owns the line
                            if (result.actors.Contains(characters))
                            {
                                //switch for whether the node is a reply or a statement
                                if (characters != "Player")
                                {
                                    // the next segment will be the line of dialogue and the one after that will be the command to be executed
                                    node created = new node();
                                    // find length
                                    end = characters.Length;
                                    // assign owner of node
                                    created.owner = characters.Substring(0, end);
                                    // assign the line of the current dialogue for this node
                                    created.lineNum = offset;
                                    end = segments[k + 1].LastIndexOf('\"');
                                    // assign line of dialogue
                                    created.line = segments[k + 1].Substring(1, end - 1);
                                    //assign scenenum
                                    created.sceneNum = curScene;
                                    //check to make sure that there are commands since not all lines will have them
                                    if (segments.Length >= k + 2)
                                    {
                                        //node that this node links to
                                        //grab the jump number from inside the segment
                                        string t = segments[k + 2];
                                        int p = t.LastIndexOf(']');
                                        t = t.Substring(1, p - 1);
                                        if (t != "")
                                        {
                                            created.jumpNum = int.Parse(t);
                                        }
                                    }
                                    created.reactions = new List<reply>();
                                    result.nodes.Add(created);
                                    result.pc = result.nodes.Count - 1;
                                }

                                //if the node is a reply different parsing structure
                                //to accomodate the multiple replies
                                //this branch should only occur if the owner is player
                                else
                                {
                                    //make sure that that the player is the author of the replies trying to be generated
                                    if(characters != "Player") {
                                        System.Console.WriteLine("Warning error detected....attempted generation of replies that don't belong to player.");
                                    }
                                    //make sure that a parent node exists for replies
                                    //also serves as error correction
                                    if(result.nodes[result.pc] == null)
                                    {
                                        System.Console.WriteLine("Warning error detected.... attempted generation of replies without a parent statement.");
                                    }
                                   
                                    //now that we have checked for errors we can parse 
                                    //loop through the rest of the lines in this segment to get replies
                                    //start loop after where the player was first detected (j)
                                    for(int m = j+2; m< lines.Length; m++)
                                    {
                                        if(lines[m] == "")
                                        {
                                            continue;
                                        }
                                        //remove first character
                                        string tmpline = lines[m].Substring(1, lines[m].Length - 1);
                                        reply tmpreply = new reply();
                                        tmpreply.sceneNum = curScene;

                                        //split line on tabs
                                        //then assign jump num and line
                                        segments = lines[m].Split('\t');
                                        // assign line of dialogue
                                        int stop = segments[1].LastIndexOf('\"');
                                        tmpreply.line = segments[1].Substring(1, stop - 1);
                                        //check to make sure that there are commands since not all lines will have them
                                        if (segments.Length >= 2)
                                        {
                                            //node that this node links to
                                            //grab the jump number from inside the segment
                                            string t = segments[2];
                                            int p = t.LastIndexOf(']');
                                            t = t.Substring(1, p - 1);
                                            // need some error checking 

                                            if (t != "")
                                            {
                                                tmpreply.jumpNum = int.Parse(t);
                                            }
                                        }
                                        result.nodes[result.pc].reactions.Add(tmpreply);
                                    }

                                }
                            }


                        }
                    }
                }
            }





            return result;
        }

        //create a text file based on a dialogue tree
        static void transcribe(tree tbt, string filename, string path)
        {
            //create full path
            path += filename += ".txt";
            //create a file to transcribe tree to
            using (StreamWriter scribe = File.CreateText(path))
            {
                scribe.WriteLine("#" + tbt.title + "#");

                //to be added at later date when gates and stuff implemented
                //loop through variables in tree
                for (int i = 0; i < tbt.flags.Count(); i++)
                {
                    scribe.Write("<" + tbt.flags[i].name + ">");
                    scribe.Write('\t');
                }
                //new line
                scribe.Write('\n');
                //loop through the actors in the tree
                for (int i = 0; i < tbt.actors.Count(); i++)
                {
                    scribe.Write("|" + tbt.actors[i] + "|");
                    scribe.Write('\t');
                }
                //new line
                scribe.Write('\n');

                //loop for each node
                for(int i = 0; i < tbt.nodes.Count(); i++)
                {
                    //trancribe tags for start and end of each node
                    scribe.WriteLine("~");
                    //transcribe node number
                    scribe.WriteLine("{" + tbt.nodes[i].sceneNum + "}");
                    scribe.Write(tbt.nodes[i].owner);
                    scribe.Write('\t');
                    scribe.Write('"' + tbt.nodes[i].line + '"');
                    scribe.Write('\t');
                    scribe.Write("[" + tbt.nodes[i].jumpNum + "]");
                    scribe.Write('\n');
                    //loop to handle reactions
                    //write owner for list of reactions
                    bool check = false;
                    for (int j = 0; j< tbt.nodes[i].reactions.Count(); j++)
                    {
                        //check to see if owner was initially printed
                        if (!check)
                        {
                            scribe.WriteLine(tbt.nodes[i].reactions[j].owner);
                            check = true;
                        }
                        //write tab
                        scribe.Write('\t');
                        scribe.Write('"' +tbt.nodes[i].reactions[j].line +'"');
                        scribe.Write('\t');
                        scribe.Write("[" + tbt.nodes[i].reactions[j].jumpNum + "]");
                        scribe.Write('\n');
                    }                   
                }
                //end tag
                scribe.WriteLine("~");
            }
        }
    }
}
