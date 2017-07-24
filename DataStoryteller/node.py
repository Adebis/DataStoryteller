import pygame
from pygame import gfxdraw

class node(object):

    WHITE = (255, 255, 255)
    BLUE = (0, 0, 255)

    # A node in the graph.
    def __init__(self):
        print ("Initializing new node")
        self.color = self.BLUE
        self.border_color = self.WHITE
        self.position = (250, 250)
        self.starting_size = 10
        self.size = 10

    # Have this node draw itself on the given screen.
    def draw(self, screen):
        # Draw the borders of the node.
        pygame.gfxdraw.filled_circle(screen, self.position[0], self.position[1], self.radius, self.border_color)
        pygame.gfxdraw.aacircle(screen, self.position[0], self.position[1], self.radius, self.border_color)
        # Draw the body of the node
        pygame.gfxdraw.filled_circle(screen, self.position[0], self.position[1], self.radius - 2, self.color)
        pygame.gfxdraw.aacircle(screen, self.position[0], self.position[1], self.radius - 2, self.color)

