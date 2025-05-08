(define (domain robot)
  (:requirements :strips :typing :numeric-fluents :durative-actions)
  (:types room obj robot)
  (:functions(move-time  ?r - robot)(battery    ?r - robot))
  (:predicates
    (at-robot ?r - robot ?l - room) (at-obj ?b - obj ?r - room)
    (free ?r - robot) (carry ?o - obj ?r - robot)
    (allowed ?r - robot ?l - room))


  (:durative-action move
      :parameters (?r - robot ?a - room ?b - room)
      :duration (= ?duration (move-time ?r))
      :condition (and
          (over all (allowed ?r ?b))
          (at start (at-robot ?r ?a))
          (at start (> (battery ?r) 20)))
      :effect (and
          (at start (not (at-robot ?r ?a)))
          (at end (at-robot ?r ?b))
          (at end (decrease (battery ?r) 20))
      )
  )

	(:action pick
		:parameters     (?o - obj ?l - room ?r - robot)
		:precondition   (and  (at-obj ?o ?l) (at-robot ?r ?l) (free ?r))
		:effect         (and (carry ?o ?r) (not (at-obj ?o ?l))
                    (not (free ?r))))

	(:action drop
		:parameters     (?o - obj ?l - room ?r - robot)
		:precondition   (and  (carry ?o ?r) (at-robot ?r ?l))
    :effect         (and (at-obj ?o ?l) (free ?r)
                    (not (carry ?o ?r))))


)
