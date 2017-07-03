from DataStoryteller import DataStoryteller

def main():
    while (True):
        input_string = input("Input command: ")
        storyteller = DataStoryteller()

        if (input_string == "help"):
            print ("Commands: ")
            print ("quit")
            print ("    Exits the program.")
            print ("start")
            print ("    Starts the storyteller.")
        elif (input_string == "quit"):
            print ("Exiting program...")
            break
        elif (input_string == "start"):
            storyteller = DataStoryteller()
            storyteller.TellStory()
        elif (input_string == "tell story"):
            print ("Characters: ")
            for key in storyteller.yearly_citations_by_algorithm.keys():
                print ("    " + key)
            input_string = input("Input character name: ")
            storyteller.TellCharacterStory(input_string)

if __name__ == "__main__":
    main()