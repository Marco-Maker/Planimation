(define (domain gripper)
(:requirements :strips :typing)
(:types room ball gripper)
(:predicates (at-robby ?r - room) ; robot is in room ?r
             (at ?b - ball ?r - room) ; ball ?b is in room ?r
             (free ?g - gripper)
             (carry ?o - ball ?g - gripper)) ; ball ?o is in gripper ?g
             

	(:action move
		:parameters  (?from ?to)
		:precondition (and (at-robby ?from))
		:effect (and  (at-robby ?to) (not (at-robby ?from))))
		
	(:action pick
		:parameters (?obj ?room ?gripper)
		:precondition  (and 
                     (at ?obj ?room) (at-robby ?room) (free ?gripper))
		:effect (and (carry ?obj ?gripper) (not (at ?obj ?room)) 
              (not (free ?gripper))))
 
	(:action drop
		:parameters  (?obj  ?room ?gripper)
		:precondition  (and 
                     (carry ?obj ?gripper) (at-robby ?room))
	:effect (and (at ?obj ?room) (free ?gripper) (not (carry ?obj ?gripper))))
)


