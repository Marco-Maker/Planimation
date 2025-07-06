(define (domain test)
  (:predicates (at ?x))
  (:action stay
    :parameters (?x)
    :precondition (at ?x)
    :effect (at ?x)))
