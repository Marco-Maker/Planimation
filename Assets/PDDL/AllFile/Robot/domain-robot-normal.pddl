(define (domain robot)
	(:requirements :strips :typing)
	(:types
		room obj robot
	)
	(:predicates
		(at-robot ?r - robot ?l - room)
		(connected ?a - room ?b - room)
		(at-obj ?b - obj ?r - room)
		(free ?r - robot)
		(carry ?o - obj ?r - robot)
		(allowed ?r - robot ?l - room)
	)

	(:action move
		:parameters (?r - robot ?a - room ?b - room)
		:precondition (and
			(at-robot ?r ?a)
			(connected ?a ?b)
			(allowed ?r ?b)
		)
		:effect (and (at-robot ?r ?b) (not (at-robot ?r ?a)))
	)

	(:action pick
		:parameters (?o - obj ?l - room ?r - robot)
		:precondition (and (at-obj ?o ?l) (at-robot ?r ?l) (free ?r))
		:effect (and (carry ?o ?r) (not (at-obj ?o ?l))
			(not (free ?r)))
	)

	(:action drop
		:parameters (?o - obj ?l - room ?r - robot)
		:precondition (and (carry ?o ?r) (at-robot ?r ?l))
		:effect (and (at-obj ?o ?l) (free ?r)
			(not (carry ?o ?r)))
	)
)