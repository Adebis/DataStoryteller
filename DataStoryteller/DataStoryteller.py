import sys
import csv
import operator
import random

class DataStoryteller:

    def __init__(self):
        print ("Initializing new DataStoryteller")

        self.character_names = {"artificial_neural_network"
                          , "decision_tree"
                          , "k-means_clustering"
                          , "linear_regression"
                          , "support_vector_machine"}

        # The key for all the following dictionaries is the name of the algorithm.
        # Create a dictionary to store the year counts of
        # each machine learning algorithm.
        self.yearly_citations_by_algorithm = {}
        # For each algorithm, keep track of which paper has the most citations,
        # when it was written, and how many citations it got.
        self.max_paper_by_algorithm = {}
        self.earliest_paper_by_algorithm = {}
        self.latest_paper_by_algorithm = {}
        self.total_citations_by_algorithm = {}
        # For each algorithm, keep the algorithm's set of events.
        self.events_by_algorithm = {}

        # A dictionary mapping Dramatis Personae to character names.
        # The key is the Dramatis Personae, the value is the character name.
        self.dramatis_personae = {}

        # The earliest and latest years that appear in these datasets
        self.minimum_year = 3000
        self.maximum_year = 0

        # Read in the CSVs
        for algorithm_name in self.character_names:
            # A dictionary of total citations for each year
            citations_by_year = {}
            # The triple of the max paper (name, year, citations)
            max_paper = {"name":"", "author":"", "year":0, "citations":0, "reason":"most cited paper"}
            # The paper that appears earliest in this algorithm's history.
            earliest_paper = {"name":"", "author":"", "year":3000, "citations":0, "reason":"earliest paper"}
            latest_paper = {"name":"", "author":"", "year":0, "citations":0, "reason":"latest paper"}
            total_citations = 0
            current_file = open(algorithm_name + ".csv", 'rt', encoding='utf8')
            csv_reader = csv.reader(current_file, delimiter=',', quotechar='"')
            first_row = True
            for row in csv_reader:
                # Skip the first row, the headers
                if first_row:
                    first_row = False
                    continue
                # Item 0 is how many citations the paper got.
                # Item 1 is the author(s).
                # Item 2 is the title.
                # Item 3 is the year the paper was published.
                #print (row)
                citation_count = row[0]
                authors = row[1]
                title = row[2]
                year = row[3]
                # Try to cast the citation count and year to integer.
                citation_count_int = int(citation_count)
                year_int = int(year)
                # Ignore the 0th year
                if year_int == 0:
                    continue

                if year_int < self.minimum_year:
                    self.minimum_year = year_int
                if year_int > self.maximum_year:
                    self.maximum_year = year_int

                # Check if the year already exists in the dictionary of citations by year.
                if not year_int in citations_by_year:
                    citations_by_year[year_int] = 0
                citations_by_year[year_int] += citation_count_int
                total_citations += citation_count_int
                
                # Check whether this beats this algorithm's max paper.
                if citation_count_int > max_paper["citations"]:
                    max_paper = {"name":title, "author":authors, "year":year_int, "citations":citation_count_int, "reason":"most cited paper"}
                # Check whether this is earlier than this algorithm's earliest paper.
                if year_int < earliest_paper["year"]:
                    earliest_paper = {"name":title, "author":authors, "year":year_int, "citations":citation_count_int, "reason":"earliest cited paper"}
                # Check whether this is later than this algorithm's latest paper.
                if year_int > latest_paper["year"]:
                    latest_paper = {"name":title, "author":authors, "year":year_int, "citations":citation_count_int, "reason":"latest cited paper"}

            # Now, add the dictionary of citations by year to the by-algorithm dictionary.
            #sorted_citations_by_year = sorted(citations_by_year.items(), key=operator.itemgetter(0))
            #self.yearly_citations_by_algorithm[algorithm_name] = sorted_citations_by_year
            self.yearly_citations_by_algorithm[algorithm_name] = citations_by_year
            
            self.max_paper_by_algorithm[algorithm_name] = max_paper
            self.earliest_paper_by_algorithm[algorithm_name] = earliest_paper
            self.latest_paper_by_algorithm[algorithm_name] = latest_paper
            self.total_citations_by_algorithm[algorithm_name] = total_citations

        print ("Citation counts by year for each algorithm finished.")
        # Now, this object's minimum year, maximum year, and yearly citations by algorithm are set.

        # Turn the data that was just made into a set of events for each character.
        for algorithm_name in self.character_names:
            character_events = self.DataToEvents(algorithm_name)
            self.events_by_algorithm[algorithm_name] = character_events

    # Transform a character's data into a set of events.
    def DataToEvents(self, character_name):
        if not character_name in self.yearly_citations_by_algorithm:
            print ("Character " + character_name + " not found in data.")
            return
        citations_by_year = self.yearly_citations_by_algorithm[character_name]
        character_name_parsed = character_name.replace("_", " ")
        # Determine the events for this character's data
        event_list = list()

        peak_year = 0
        peak_count = 0

        last_count = 0
        last_last_count = 0
        current_count = 0
        for year in range(self.minimum_year, self.maximum_year):
            # If this year does not appear in the dictionary, then no citations were made this year.
            if not year in citations_by_year:
                current_count = 0
            else:
                current_count = citations_by_year[year]
            # Check for appear event.
            # This is when the count goes from 0 to anything positive.
            if last_count == 0 and current_count > 0:
                event_list.append({"type":"appear"
                                    , "time":year
                                    , "actor":character_name_parsed})
            # Check for disappear event.
            # This is when the count goes from anything positive to 0.
            if last_count > 0 and current_count == 0:
                event_list.append({"type":"disappear"
                                    , "time":year
                                    , "actor":character_name_parsed})

            # Check for a local minimum event.
            # This is when the count before and after a time step is greater than
            # during the timestep.
            # Because we do not look ahead, instead compare the last count to the last last count and
            # the current count.
            # Note: Both equality cases are covered in case there is a plateau before or after the last year (but not both before AND after).
            if (last_last_count > last_count and current_count >= last_count) or (last_last_count >= last_count and current_count > last_count):
                event_list.append({"type":"local_minimum"
                                   , "time":year - 1
                                   , "actor":character_name_parsed})
            # Do a similar check for the local maximum event.
            if (last_last_count < last_count and current_count <= last_count) or (last_last_count <= last_count and current_count < last_count):
                event_list.append({"type":"local_maximum"
                                   , "time":year - 1
                                   , "actor":character_name_parsed})

            # Keep track of the peak year
            if current_count > peak_count:
                peak_year = year
                peak_count = current_count
            # Update the last last count to the current last count.
            last_last_count = last_count
            # Update last count to the current count.
            last_count = current_count

        # Create peak event (year when the most total citations occurred)
        event_list.append({"type":"peak"
                           , "time":peak_year
                           , "actor":character_name_parsed
                           , "data":{"citations":peak_count}})

        # Create peak paper event (when the single paper with the most citations was published)
        max_paper = self.max_paper_by_algorithm[character_name]
        event_list.append({"type":"max_paper"
                           , "time":max_paper["year"]
                           , "actor":max_paper["name"] 
                           , "data":max_paper})
        # Create earliest paper event (when the single paper with the earliest date was published)
        earliest_paper = self.earliest_paper_by_algorithm[character_name]
        event_list.append({"type":"earliest_paper"
                    , "time":earliest_paper["year"]
                    , "actor":earliest_paper["name"]
                    , "data":earliest_paper})

        latest_paper = self.latest_paper_by_algorithm[character_name]

        # Create the first appear event.
        for event in event_list:
            if event["type"] == "appear":
                event["type"] = "first_appear"
                break
        # Create the first disappear event.
        for event in event_list:
            if event["type"] == "disappear":
                event["type"] = "first_disappear"
                break
        # Create last appear and last disappear event (the last time the character makes an appearance)
        # Go backwards through the event list, looking for the first appear and first disappear event we can find.
        event_list.reverse()
        last_appear_made = False
        last_disappear_made = False
        for event in event_list:
            if event["type"] == "appear":
                event["type"] = "last_appear"
                last_appear_made = True
                if last_disappear_made:
                    break
            elif event["type"] == "disappear":
                event["type"] = "last_disappear"
                last_disappear_made = True
                if last_appear_made:
                    break
        event_list.reverse()

        return event_list

    # Find and return the first event in a certain character's event list of the given type.
    # Returns None if no such event is found.
    def FindEvent(self, character_name, event_type):
        event_list = self.events_by_algorithm[character_name]
        for event in event_list:
            if event["type"] == event_type:
                return event
        return None
    # Find and return all events in a certain character's event list of the given type
    # Returns None if no such event is found.
    def FindAllEvents(self, character_name, event_type):
        return_list = []
        event_list = self.events_by_algorithm[character_name]
        for event in event_list:
            if event["type"] == event_type:
                return_list.append(event)
        if (len(return_list) > 0):
            return return_list
        return None

    # Tell the entire story.
    def TellStory(self):
        print ("Telling story.")
        print ("Characters: ")
        for character_name in self.character_names:
            print ("    " + character_name)

        print ("Which character will be the hero?")
        while (True):
            user_input = input("Enter the name of the character you wish to be the hero. Type 'quit' to stop: ")
            if (user_input in self.character_names):
                self.dramatis_personae["hero"] = user_input
                break
            elif (user_input == 'quit'):
                print ("Exiting story.")
                return
            else:
                print (user_input + " is not a valid character name.")

        # Whether or not the story is finished.
        story_finished = False

        # To create a Proppian story, we string together a set of Functions.
        # Each Function has Dramatis Personae that take part in the Events of the function.
        # The functions:
        #   1. Initial situation - The members of a
        #       family are enumerated, or the future hero (e.g., a soldier) is simply introduced by
        #       mention of his name or indication of his status.
        #   2. One of the members of a family absents himself from home.
        #   3. Lack - a misfortune or lack is made known.
        #   3. Dispatch - the hero is dispatched.
        #   4. Challenge - the hero is tested.
        #   5. Receipt - the hero receives a magical agent.
        #   6. Liquidation - the initial misfortune or lack is liquidated.
        #   7. Reward - the hero is rewarded.
        #   8. Final situation - the epilogue, wherein the events of the story are concluded.
        general_function_sequence = []
        general_function_sequence.append("initial_situation")
        general_function_sequence.append("absentation")
        general_function_sequence.append("lack")
        #general_function_sequence.append("dispatch")
        #general_function_sequence.append("challenge")
        general_function_sequence.append("receipt")
        general_function_sequence.append("liquidation")
        #general_function_sequence.append("reward")
        general_function_sequence.append("final_situation")

        # The specific functions of this specific story. 
        story_functions = []
        # The first function is the Initial Situation.
        function_entry = {}
        function_entry["name"] = "initial_situation"
        function_entry["personae"] = {}
        # Set the hero decided by the user as the hero for the initial situation.
        function_entry["personae"]["hero"] = self.dramatis_personae["hero"]
        # Look for family members of the hero.
        # For now, just set every other algorithm as the family of the hero.
        function_entry["personae"]["family_members"] = []
        for character_name in self.character_names:
            if not character_name == function_entry["personae"]["hero"]:
                function_entry["personae"]["family_members"].append(character_name)
                self.dramatis_personae["family_members"] = function_entry["personae"]["family_members"]
        story_functions.append(function_entry)
        
        last_function = function_entry

        # Enter a forward planning loop. From the last function that was made, try to make a function further ahead.

        while (not story_finished):
            last_function_index = general_function_sequence.index(last_function["name"])
            # Greedily try functions that appear after the last function in the general function sequence.
            # Any function that COULD come after the last function should be tried until one is found that fits.
            for i in range(last_function_index + 1, len(general_function_sequence)):
                function_to_try_name = general_function_sequence[i]
                # A family member absents him/herself from home.
                if (function_to_try_name == "absentation"):
                    # For absentation, look for a family member of the hero that last disappears before the hero last appears.
                    hero_last_appear = self.FindEvent(self.dramatis_personae["hero"], "last_appear")
                    # if there is no last appear, take the first appear instead.
                    if hero_last_appear == None:
                        hero_last_appear = self.FindEvent(self.dramatis_personae["hero"], "first_appear")
                    family_member_names = self.dramatis_personae["family_members"]
                    absentee_name = ""
                    for family_member_name in family_member_names:
                        family_last_disappear = self.FindEvent(family_member_name, "last_disappear")
                        # If there is no last disappear, check for a first disappear.
                        if family_last_disappear == None:
                            family_last_disappear = self.FindEvent(family_member_name, "first_disappear")
                        # If it is still none, this family member has never disappeared. Give up on this family member.
                        if family_last_disappear == None:
                            break
                        if family_last_disappear["time"] <= hero_last_appear["time"]:
                            # We have a match.
                            absentee_name = family_member_name
                            break
                    # If there is an absentee name that is not the empty string, then an absentee was found.
                    # The function is valid, so create it and add it to the list of story functions.
                    if not absentee_name == "":
                        function_entry = {}
                        function_entry["name"] = "absentation"
                        function_entry["personae"] = {}
                        function_entry["personae"]["hero"] = self.dramatis_personae["hero"]
                        function_entry["personae"]["absentee"] = absentee_name
                        self.dramatis_personae["absentee"] = absentee_name
                        function_entry["events"] = []
                        function_entry["events"].append(self.FindEvent(absentee_name, "last_disappear"))
                        story_functions.append(function_entry)
                        last_function = function_entry
                        # Break the outer for loop so we don't accidentally go on to check further functions.
                        break
                # The hero or a family member lacks something or encounters a misfortune.
                if (function_to_try_name == "lack"):
                    lacker_name = ""
                    # Selfish lack: The hero is the lacker.
                    lacker_name = self.dramatis_personae["hero"]
                    # Find the earliest local minimum after the lacker's 
                    # last appearance and before the lacker's peak.
                    local_minima = self.FindAllEvents(lacker_name, "local_minimum")
                    lacker_last_appear = self.FindEvent(lacker_name, "last_appear")
                    # If there is no last appear, take the first appear.
                    if lacker_last_appear == None:
                        lacker_last_appear = self.FindEvent(lacker_name, "first_appear")
                    lacker_peak = self.FindEvent(lacker_name, "peak")
                    lack_event = None
                    for minimum_event in local_minima:
                        if minimum_event["time"] > lacker_last_appear["time"] and minimum_event["time"] < lacker_peak["time"]:
                            lack_event = minimum_event
                            break
                    # If we have found a lack event, then it this function is valid.
                    if not lack_event == None:
                        function_entry = {}
                        function_entry["name"] = "lack"
                        function_entry["personae"] = {}
                        function_entry["personae"]["lacker"] = lacker_name
                        self.dramatis_personae["lacker"] = lacker_name
                        function_entry["events"] = []
                        function_entry["events"].append(lack_event)
                        story_functions.append(function_entry)
                        last_function = function_entry
                        # Break the outer for loop so we don't accidentally go on to check further functions.
                        break
                # The hero receives a magical agent.
                if (function_to_try_name == "receipt"):
                    # Look for the hero's most cited paper before the peak.
                    # The most cited paper itself represents the magical agent.
                    receipt_event = None
                    max_paper = self.FindEvent(self.dramatis_personae["hero"], "max_paper")
                    peak = self.FindEvent(self.dramatis_personae["hero"], "peak")
                    if max_paper["time"] < peak["time"]:
                        receipt_event = max_paper
                    if not receipt_event == None:
                        function_entry = {}
                        function_entry["name"] = "receipt"
                        function_entry["personae"] = {}
                        function_entry["personae"]["hero"] = self.dramatis_personae["hero"]
                        function_entry["personae"]["magical_agent"] = max_paper["actor"]
                        self.dramatis_personae["magical_agent"] = max_paper["actor"]
                        function_entry["events"] = []
                        function_entry["events"].append(receipt_event)
                        story_functions.append(function_entry)
                        last_function = function_entry
                        # Break the outer for loop so we don't accidentally go on to check further functions.
                        break
                if (function_to_try_name == "liquidation"):
                    # Look for the lacker's peak event.
                    liquidation_event = None
                    liquidation_event = self.FindEvent(self.dramatis_personae["lacker"], "peak")
                    if not liquidation_event == None:
                        function_entry = {}
                        function_entry["name"] = "liquidation"
                        function_entry["personae"] = {}
                        function_entry["personae"]["lacker"] = self.dramatis_personae["lacker"]
                        function_entry["events"] = []
                        function_entry["events"].append(liquidation_event)
                        story_functions.append(function_entry)
                        last_function = function_entry
                        # Break the outer for loop so we don't accidentally go on to check further functions.
                        break
                if (function_to_try_name == "final_situation"):
                        function_entry = {}
                        function_entry["name"] = "final_situation"
                        function_entry["personae"] = {}
                        function_entry["events"] = []
                        story_functions.append(function_entry)
                        last_function = function_entry
                        # Since we've reached the final situation, end the planning loop here.
                        story_finished = True
                        # Break the outer for loop so we don't accidentally go on to check further functions.
                        break
        print ("Story sequence finished.")
        print ("Creating story text...")

        # Create text for each story function.
        for story_function in story_functions:
            function_text = ""
            if story_function["name"] == "initial_situation":
                function_text = "This is a story about " + story_function["personae"]["hero"] + ", a machine learning algorithm. Other machine learning algorithms were also created, like"
                family_members = story_function["personae"]["family_members"]
                for family_member in family_members:
                    if family_members.index(family_member) == len(family_members) - 1:
                        function_text += ", and " + family_member
                    else:
                        function_text += ", " + family_member
            elif story_function["name"] == "absentation":
                absentation_event = story_function["events"][0]
                function_text = "One day, in " + str(absentation_event["time"]) + ", " + story_function["personae"]["absentee"] + " a machine learning algorithm like " + self.dramatis_personae["hero"] + ", disappeared, getting no citations for papers written that year."
            elif story_function["name"] == "lack":
                lack_event = story_function["events"][0]
                function_text = "In " + str(lack_event["time"]) + ", " + story_function["personae"]["lacker"] + ", itself, was experiencing a downturn in interest, getting less citations that year than the years before or after."
            elif story_function["name"] == "receipt":
                receipt_event = story_function["events"][0]
                function_text = "Then, in " + str(receipt_event["time"]) + ", " + receipt_event["data"]["author"] + " wrote " + receipt_event["data"]["name"] + ", the most cited paper for the algorithm to date with " + str(receipt_event["data"]["citations"]) + " citations."
                # max_paper = {"name":title, "author":authors, "year":year_int, "citations":citation_count_int, "reason":"most cited paper"}
            elif story_function["name"] == "liquidation":
                liquidation_event = story_function["events"][0]
                function_text = "Fortunately for " + story_function["personae"]["lacker"] + ", in " + str(liquidation_event["time"]) + ", interest came soaring back, peaking with " + str(liquidation_event["data"]["citations"]) + " citations; the most the field has ever seen about the algorithm."
            elif story_function["name"] == "final_situation":
                function_text = "The End."
            print (function_text)
    # Tell a story about a single character.
    def TellCharacterStory(self, character_name):
        print ("Telling story about " + character_name)
        if not character_name in self.yearly_citations_by_algorithm:
            print ("Character " + character_name + " not found")
            return
        event_list = DataToEvents(character_name)

        character_name_parsed = character_name.replace("_", " ")

        # SIMPLE STORYTELLING: Present chronologically.
        # Order events chronologically
        chronological_event_list = sorted(event_list, key=operator.itemgetter("time"))
        # Introduction: give some hints as to what will happen in the story and introduce the character.
        number_to_hint = 3
        turn_text = "This is a story about " + character_name_parsed
        random_event_indices = random.sample(range(1, len(chronological_event_list)), number_to_hint)
        number_mentioned = 0
        events_mentioned = list()
        for event_index in random_event_indices:
            number_mentioned += 1
            random_event = chronological_event_list[event_index - 1]
            if (random_event["type"] in events_mentioned):
                continue
            if (number_mentioned == number_to_hint):
                turn_text += ", and "
            else:
                turn_text += ", "

            if (random_event["type"] == "influential"):
                turn_text += "what papers were influential to it"
            else:
                turn_text += "when it " + random_event["type"].replace("_", " ") + "ed"
            events_mentioned.append(random_event["type"])

            if (number_mentioned == number_to_hint):
                turn_text += "."

        print (turn_text.encode(sys.stdout.encoding, errors='replace'))

        previous_time = 0
        for event in chronological_event_list:
            turn_text = ""
            # Check if the times are the same. If so, refer to this as the same year.
            time_gap = event["time"] - previous_time
            if (previous_time == event["time"]):
                turn_text += "That very same year, "
            elif (time_gap == 1):
                turn_text += "The next year, "
            elif (time_gap <= 5):
                time_gap = event["time"] - previous_time
                turn_text += str(time_gap) + " years later, in " + str(event["time"]) + ", "
            else:
                turn_text += "In " + str(event["time"]) + ", "

            if event["type"] == "first_appear":
                turn_text += event["actor"] + " was first mentioned. "
            elif event["type"] == "first_disappear":
                turn_text += event["actor"] + " was nowhere to be seen."
            elif event["type"] == "influential":
                turn_text += character_name_parsed + "'s " + event["data"]["reason"] + ", " + event["actor"] + ", " + "was written by " + event["data"]["author"] + ", getting " + str(event["data"]["citations"]) + " citations."
            elif event["type"] == "peak":
                turn_text += event["actor"] + " peaked with " + str(event["data"]["citations"]) + " citations."
            elif event["type"] == "appear":
                turn_text += event["actor"] + " was mentioned again."
            elif event["type"] == "disappear":
                turn_text += event["actor"] + " disappeared again."
            elif event["type"] == "last_disappear":
                turn_text += event["actor"] + " made its last disappearance."
            elif event["type"] == "last_appear":
                turn_text += event["actor"] + " appeared and stayed."
            else:
                turn_text += event["actor"] + " " + event["type"] + ". "
            print (turn_text.encode(sys.stdout.encoding, errors='replace'))
            previous_time = event["time"]
        # Epilogue: give a small summary of where the character is currently.
        # This includes last-cited paper and accrued statistics.
        turn_text = "Today, "
        turn_text += character_name_parsed + " was last mentioned in " + latest_paper["name"] + " by " + latest_paper["author"] + ", written in " + str(latest_paper["year"]) + " and getting " + str(latest_paper["citations"]) + " citations. "
        turn_text += "Overall, " + character_name_parsed + " has been cited " + str(self.total_citations_by_algorithm[character_name]) + " times between " + str(earliest_paper["year"]) + " and " + str(latest_paper["year"]) + "."
        print (turn_text.encode(sys.stdout.encoding, errors='replace'))