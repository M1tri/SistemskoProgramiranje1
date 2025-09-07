import os
import random
import string

MAX_DUBINA = 6
MAX_PODDIR = 5
MAX_FAJL = 3

def get_ime():
    broj = random.randint(0, 4)

    match (broj):
        case 0:
            return "fajlic"
        case 1:
            return "fajl"
        case 2:
            return "file"
        case 3:
            return "zuto"
        case 4:
            return "nora"


def get_tekst():
    strofe = [
        # 0
            """[Verse 1]
    Transport, motorways and tramlines
    Starting and then stopping
    Taking off and landing
    The emptiest of feelings
    Disappointed people
    Clinging onto bottles
    And when it comes
    It's so, so disappointing""",

            # 1
            """[Chorus]
    Let down and hanging around
    Crushed like a bug in the ground
    Let down and hanging around""",

            # 2
            """[Verse 2]
    Shell smashed, juices flowing
    Wings twitch, legs are going
    Don't get sentimental
    It always ends up drivel
    One day, I am gonna grow wings
    A chemical reaction
    Hysterical and useless
    Hysterical and""",

            # 3
            """[Chorus]
    Let down and hanging around
    Crushed like a bug in the ground
    Let down and hanging around""",

            # 4
            """[Bridge]
    Let down and hang
    Let down and hang
    Let down and hang""",

            # 5
            """[Verse 3]
    You know, you know where you are with
    You know where you are with
    Floor collapsing
    Floating, bouncing back
    And one day, I am gonna grow wings
    A chemical reaction (You know where you are)
    Hysterical and useless (You know where you are)
    Hysterical and (You know where you are)""",

            # 6
            """[Chorus]
    Let down and hanging around
    Crushed like a bug in the ground
    Let down and hanging around""",

            # 7
            """[Verse 1]
    That there
    That's not me
    I go
    Where I please
    I walk through walls
    I float down the Liffey""",

            # 8
            """[Chorus]
    I'm not here
    This isn't happening
    I'm not here
    I'm not here""",

            # 9
            """[Verse 2]
    In a little while
    I'll be gone
    The moment's already passed
    Yeah, it's gone""",

            # 10
            """[Chorus]
    And I'm not here
    This isn't happening
    I'm not here
    I'm not here""",

            # 11
            """[Bridge]
    Strobe lights
    And blowing speakers
    Fireworks
    And hurricanes""",

            # 12
            """[Chorus]
    I'm not here
    This isn't happenin'
    I'm not here
    I'm not here""",

            # 13
            """[Outro]
    Ah-ah, ah-ah
    Ah-ah, ah-ah
    Ah-ah
    Ah-ah, ah-ah-ah
    Ah-ah-ah
    Ah-ah-ah
    Ah-ah-ah, ah-ah"""
    ]

    i = random.randint(0, len(strofe) - 1)
    return strofe[i]


broj_fajla = 0
broj_dira = 0

def generisi(path, dubina):
    global broj_fajla, broj_dira
    
    broj_fajlova = random.randint(0, MAX_FAJL)

    for i in range(broj_fajlova):
        file_name = get_ime() + str(broj_fajla) + ".txt"
        broj_fajla = broj_fajla + 1
        file_path = os.path.join(path, file_name)
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(get_tekst())

    if (dubina > MAX_DUBINA):
        return

    broj_pod_dirova = random.randint(1, MAX_PODDIR)

    for i in range(broj_pod_dirova):
        dir_name = "dir" + str(broj_dira)
        broj_dira += 1
        dir_path = os.path.join(path, dir_name)
        os.makedirs(dir_path, exist_ok=True)
        generisi(dir_path, dubina+1)

if __name__ == "__main__":
    generisi(".", 0)    

