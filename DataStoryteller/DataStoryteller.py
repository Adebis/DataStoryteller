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

        # Create a dictionary to store the year counts of
        # each machine learning algorithm.
        self.yearly_citations_by_algorithm = {}
        # For each algorithm, keep track of which paper has the most citations,
        # when it was written, and how many citations it got.
        self.max_paper_by_algorithm = {}
        self.earliest_paper_by_algorithm = {}
        self.latest_paper_by_algorithm = {}
        self.total_citations_by_algorithm = {}

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

    def TellStory(self, character_name):
        print ("Telling story about " + character_name)
        if not character_name in self.yearly_citations_by_algorithm:
            print ("Character " + character_name + " not found")
            return
        citations_by_year = self.yearly_citations_by_algorithm[character_name]
        character_name_parsed = character_name.replace("_", " ")
        # Determine the events for this character's data
        event_list = list()

        peak_year = 0
        peak_count = 0

        last_count = 0
        current_count = 0
        for year in range(self.minimum_year, self.maximum_year):
            # If this year does not appear in the dictionary, then no citations were made this year.
            if not year in citations_by_year:
                current_count = 0
            else:
                current_count = citations_by_year[year]
            # Check for appear event
            if last_count == 0 and current_count > 0:
                event_list.append({"type":"appear"
                                    , "time":year
                                    , "actor":character_name_parsed})
            # Check for disappear event
            if last_count > 0 and current_count == 0:
                event_list.append({"type":"disappear"
                                    , "time":year
                                    , "actor":character_name_parsed})
            # Keep track of the peak year
            if current_count > peak_count:
                peak_year = year
                peak_count = current_count
            # Update last count to the current count.
            last_count = current_count

        # Create peak event (year when the most total citations occurred)
        event_list.append({"type":"peak"
                           , "time":peak_year
                           , "actor":character_name_parsed
                           , "data":{"citations":peak_count}})

        # Create peak paper event (when the single paper with the most citations was published)
        max_paper = self.max_paper_by_algorithm[character_name]
        event_list.append({"type":"influential"
                           , "time":max_paper["year"]
                           , "actor":max_paper["name"] 
                           , "data":max_paper})
        # Create earliest paper event (when the single paper with the earliest date was published)
        earliest_paper = self.earliest_paper_by_algorithm[character_name]
        event_list.append({"type":"influential"
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